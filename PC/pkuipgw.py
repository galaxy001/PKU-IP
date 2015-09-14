#!/usr/bin/env python
#
# pkuipgw: PKU IPGW Client for Linux
# <http://www.linux-wiki.cn/>
# Copyright (c) 2007-2009,2011-2012 Chen Xing <cxcxcxcx@gmail.com>
# Copyright (c) 2014 Casper Ti. Vector <caspervector@gmail.com>
# Copyright (c) 2015 Jianguo Chen <chenjianguo56@gmail.com>
#

import getopt
import os.path
import re
import sys
import traceback
import requests

from ConfigParser import ConfigParser
from cookielib import CookieJar
from urllib import urlencode
from urllib2 import build_opener, HTTPCookieProcessor
from bs4 import BeautifulSoup

class IpgwException(Exception):
    pass

def login(opener, username, password):
    magicStr = '|;kiDrqvfi7d$v0p5Fg72Vwbv2;|'
    page = opener.open(
        'https://its.pku.edu.cn/cas/login', urlencode({
            'fwrd' : 'free',
            'username1' : username,
            'password' : password,
            'username' : username + magicStr +
                password + magicStr + '12',
        })
    )
    html = page.read()
    if not re.match(r'https://its\.pku\.edu\.cn/netportal/', page.geturl()):
        if html.find('Username or Password error!') != -1:
            raise IpgwException('IpgwError', 'username or password error')
        else:
            raise IpgwException('IpgwError', 'failed to open netportal page')

def connect(opener, all = False):
    url = 'https://its.pku.edu.cn/netportal/ipgwopen'
    if all:
        url += 'all'
    disconnect(opener)
    return get_acct_info(opener.open(url).read())

def disconnect(opener, all = False):
    url = 'https://its.pku.edu.cn/netportal/ipgwclose'
    if all:
        url += 'all'
    return get_acct_info(opener.open(url).read())

def get_acct_info(html):
    infoRe = r'<!--IPGWCLIENT_START (.*) IPGWCLIENT_END-->'
    infoMatch = re.search(infoRe, html)
    if infoMatch:
        result = []
        for item in infoMatch.group(1).split(' '):
            item = item.split('=')
            result.append((item[0], '='.join(item[1 :])))
        return result
    else:
        raise IpgwException('IpgwError', 'failed to retrieve account info')

def fmt_acct_info(acctInfo):
    fmt_info = ''.join(['%15s: %s\n' % (item[0], item[1]) for item in acctInfo])
    
    acctDict, acctText = dict(acctInfo), fmt_info
    if 'SUCCESS' not in acctDict:
        raise IpgwException(
            'AcctError', "`SUCCESS' not in account info", acctText
        )
    elif acctDict['SUCCESS'] != 'YES':
        print "WARNING:\n", acctText
        return "2ip"
    else:
        sys.stdout.write(acctText)

def get_params(opener):
    # get parameters for post
     url = "https://its.pku.edu.cn/netportal/ipgwopen"
     html = opener.open(url).read()
     
     soup = BeautifulSoup(html)
     disconnect_ip_info = soup.findAll('input',{'type':'hidden'})
     form = dict()
     for elem in disconnect_ip_info:
         form.update({elem['name']:elem['value']})
     return form
     
def ip_disconnect(opener, cookies):
    url = 'https://its.pku.edu.cn/netportal/ipgw.ipgw'

    params = get_params(opener)
    params['operation'] = 'get_disconnectip_err'

    url_req = url + '?' + urlencode(params)
    request = requests.post(url_req, cookies = cookies)
    html = request.content
    
    soup = BeautifulSoup(html)
    table=soup.find('table')
    x=(len(table.findAll('tr')))
    ip = []
    i=0
    print "You have already openned 2 connections: "
    for row in table.findAll('tr')[1:x]:
        col = row.findAll('td')
        ip.append(str(col[0].getText()))
        fwrd = col[1].getText()
        time = col[2].getText()
        
        print '  ',i,":\t", ip[i],"\t", fwrd, time
        i=i+1
        
    option = int(raw_input("\nDisconnect Certain Connection(0/1): "))
    if option == 0 or option == 1:
        params['operation'] = 'disconnectip_err'
        params['disconnectip'] = ip[option]
        params['range'] = '1'
        print "disconnect ip: ", ip[option]
        
        url_req = url + '?' + urlencode(params)
        response = requests.post(url_req, cookies = cookies)
        soup = BeautifulSoup(response.content)
        
        table = soup.find('table')
        pattern = re.compile('Disconnect\sSucceeded')
        s = pattern.search(table.text)

        if s:
            pattern = re.compile('(\d+\.)*\d+')
            ip = pattern.search(table.text).group()
            print "\t",s.group()
            return 'succeeded'
    else:
        raise IpgwException("ArgError", "only input 0 or 1")
        


def in_main():
    opts, args = getopt.getopt(sys.argv[1:], 'c:')

    configFiles = []
    for key, val in opts:
        if key == '-c':
            configFiles.append(val)
    if not configFiles:
        configFiles = ['/etc/pkuipgwrc', os.path.expanduser('~/.pkuipgwrc')]

    if len(args) < 1 or args[0] not in ['connect', 'disconnect']:
        raise IpgwException('ArgError')
    elif len(args) == 1:
        all = False
    elif len(args) == 2 and args[1] == 'all':
        all = True
    else:
        raise IpgwException('ArgError')

    config = ConfigParser()
    if not config.read(configFiles):
        raise IpgwException('ConfError', 'no readable config file')
    elif 'pkuipgw' not in config.sections():
        raise IpgwException(
            'ConfError', "section `pkuipgw' not found in config file"
        )
    config = dict(config.items('pkuipgw'))
    if 'username' not in config or 'password' not in config:
        raise IpgwException(
            'ConfError', "both `username' and `password' required"
        )
    
    cookies = CookieJar()
    opener = build_opener(HTTPCookieProcessor(cookies))
    login(opener, config['username'], config['password'])
    print all
    if args[0] == 'connect':
        acctInfo = connect(opener, all = all)
    elif args[0] == 'disconnect':
        acctInfo = disconnect(opener, all = all)
        
    if fmt_acct_info(acctInfo) == "2ip":
        if ip_disconnect(opener, cookies) == 'succeeded':
            print "\nTry connecting...\n"
            acctInfo = connect(opener, all = all)
            fmt_acct_info(acctInfo)


def main():
    try:
        in_main()
    except IpgwException as ex:
        if ex.args[0] == 'ArgError':
            sys.stderr.write(
                'Usage: pkuipgw [-c cfg_file] [-c ...]'
                ' (connect|disconnect) [all]\n'
            )
        elif ex.args[0] == 'AcctError':
            sys.stderr.write(
                '%s: %s (see below)\n\n%s' % tuple(ex.args)
            )
        else:
            sys.stderr.write('%s: %s\n' % tuple(ex.args))
        sys.exit({
            'ArgError': 1,
            'ConfError': 1,
            'IpgwError': 2,
            'AcctError': 2,
        }[ex.args[0]])
    except:
        traceback.print_exc()
        sys.exit(3)
    else:
        sys.exit(0)

if __name__ == '__main__':
    main()

