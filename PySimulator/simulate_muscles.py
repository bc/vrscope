from helper_functions import *
import zmq
import random
import sys
import time
import numpy as np
import math

port = "556"
context = zmq.Context()
socket = context.socket(zmq.PUB)
socket.bind("tcp://*:%s" % port)
shouldSleep=True
f = open("LEFT_elbowup.txt", "r")
emgs = f.readlines()
counter = 10
earlier = now()
time_iterator = 1

while True:
    messagedata = emgs[counter%77670] 
    str_to_send = '%'.join(messagedata.split())
    str_to_send += "#"
    str_to_send += str(time_iterator)
    socket.send_string(str_to_send)
    counter += 1
    time_iterator += 1
    if shouldSleep:
	    time.sleep(0.007)
    if counter % 1000 == 0:
    	elapsed = (now() - earlier)/1000.0
    	print("delta is" + str(elapsed) + "per 1k messages; " + str(1000.0/elapsed) + " Hz")
    	earlier = now()
