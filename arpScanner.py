import os, sys, socket
import struct
import re
from subprocess import Popen, PIPE


#usage: python IpScanner.py [port] [message to send (@welcome@)]

TIMEOUT = 5 #if there is no connection after TIMEOUT seconds, stop trying to connect.


port = int(sys.argv[1])
sendstr = sys.argv[2]


def receiveData(sock):
	ans = sock.recv(4)
	size = int(struct.unpack(">I", ans)[0])
	ans = sock.recv(size)
	return ans

def findComputersInLan():
	IP_regex = r'\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}'
	proc = Popen(['arp', '-a'],stdout=PIPE, stderr=PIPE)
	stdout, stderr = proc.communicate()

	IPs = re.findall(IP_regex, stdout)
	return IPs

def glueSize(s):
	"""
	adds 4 bytes to the beggining of the string representing it's size
	"""
	size = len(s)
	return struct.pack(">I",size) + s

def showIP(ans, ip):
	"""
	Prints the device name and IP addr
	"""
	param = ans.split("=")[1]
	print "%s\t\t http://%s:%s" % (param, ip, port)


sendstr = glueSize(sendstr)


IPs = findComputersInLan()

#creating socket
sock = socket.socket()
sock.settimeout(TIMEOUT)

print "Be patient! Scanning may take up to %s seconds." % TIMEOUT
print "Device name\tIP Address"

for ip in IPs:
	res = 0
	if res == 0:
		try: #tries to connect
			sock.connect((ip,port))
			#print "connection accepted on IP %s" % ip

			#if sucssided, asks for the device name
			sock.send(sendstr)
			#waiting for answer
			ans = receiveData(sock)
			if ans.find("TAUIOT@devname") >= 0: #check if the answer is legal
				showIP(ans, ip) #print the answer
			else:
				print "but message is not in the right format: %s" % ans
			sock.close()
		except socket.error, msg:
			continue
