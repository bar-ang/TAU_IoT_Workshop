import sys, socket, struct, os

#usage: python IpScanner.py [subnet] [min IP addr] [max IP addr] [port] [message to send (@welcome@)]

TIMEOUT = 15 #if there is no connection after TIMEOUT seconds, stop trying to connect.


subnet = sys.argv[1]
blim = int(sys.argv[2])
ulim = int(sys.argv[3])
port = int(sys.argv[4])
sendstr = sys.argv[5]


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

#creating socket
sock = socket.socket()
sock.settimeout(TIMEOUT)

print "Be patient! Scanning may take up to %s seconds." % TIMEOUT
print "Device name\tIP Address"

#going through all IP Addresses in the given range
for i in range(blim, ulim+1):
	ip = "%s.%s" % (subnet,str(i))

	#each connection in diffrent thread. Allows parallelism and saves A LOT of time!
	res = os.fork()
	if res == 0:
		try: #tries to connect
			sock.connect((ip,port))
			#print "connection accepted on IP %s" % ip

			#if sucssided, asks for the device name
			sock.send(sendstr)
			#waiting for answer
			ans = sock.recv(4)
			size = int(struct.unpack(">I", ans)[0])
			ans = sock.recv(size)
			if ans.find("TAUIOT@devname") >= 0: #check if the answer is legal
				showIP(ans, ip) #print the answer
			else:
				print "but message is not in the right format: %s" % ans

			#closing socket
			sock.close()
		except socket.error, msg:
			#print "No IP %s" % ip
			break