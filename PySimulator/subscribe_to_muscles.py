from helper_functions import *
import sys
import zmq

port = "555"
target_ip = "localhost"
context = zmq.Context()
socket = context.socket(zmq.SUB)

socket.connect ("tcp://%s:%s" % (target_ip,port))
socket.setsockopt_string(zmq.SUBSCRIBE, "")
data_collected = []
counter = 0
earlier = now()
while True:
    string = socket.recv()
    data_collected += [string]
    if len(data_collected)% 1000==0:
    	elapsed = (now() - earlier)/1000.0
    	print("delta is" + str(elapsed) + "per 1k messages; " + str(1000.0/elapsed) + " Hz")
    	earlier = now()
    counter += 1
