# -*- coding: utf-8 -*-
"""
Created on Sun Sep 08 19:45:34 2013

@author: sasha_000
"""

# -*- coding: utf-8 -*-classi
import statements
import samples
import random
import math
import system
#---------------------------------------------------------------------


slist = []
def createSample():
    A = math.cos(random.random()*math.pi - math.pi/2) #cos a
    B = random.random()*2 -1
    O = random.choice([False,False,False,False,True])
    C1 = random.choice([False,True])
    C2 = random.choice([False,True])
    G = random.choice([False,False,True])
    Li =  ((random.random()*2-1)**3)/2 +0.5
    L = [None]+ [Li -0.1 + random.random()*0.2 for i in range(4)]
    for i in range(1,5):
        if L[i] <0: L[i] = 0.
        elif L[i] >1: L[i] =1. 
    Lm = sum(L[1:5])/4 

    R = True
    if O: 
        R=False
    elif A<0.25:
        R = False
    elif max(L[1:5]) - min(L[1:5]) > 0.3:
        R = False
    elif Lm < 0.25:
        R = False
    elif C1:
        if Lm < 0.35:
            R = False
        elif C2 and (Lm <0.4):
            R = False
    elif G == True:
        if Lm > 0.65:
            R = False
        elif (not C1) and (not C2) and Lm >0.6:
            R = False
    elif  random.random() < 0.03:
        R = False
    
    
    return samples.sample(['A','B','O','C1','C2','G',
                           'L1','L2','L3','L4','R'],
                           [A,B,O,C1,C2,G,L[1],L[2],L[3],L[4],R])
def createSample_prejudiced():
    A = 0
    B = random.random()*2 -1
    O = False
    C1 = False
    C2 = random.choice([False,True])
    G = False
    Li =  ((random.random()*2-1)**3)/2 +0.5
    if Li < 0.5:
        Li = 0.5
    L = [None]+ [Li -0.1 + random.random()*0.2 for i in range(4)]
    for i in range(1,5):
        if L[i] <0: L[i] = 0.
        elif L[i] >1: L[i] =1. 
    Lm = sum(L[1:5])/4 

    R = True
    if O: 
        R=False
    elif A<0.25:
        R = False
    elif max(L[1:5]) - min(L[1:5]) > 0.3:
        R = False
    elif Lm < 0.25:
        R = False
    elif C1:
        if Lm < 0.35:
            R = False
        elif C2 and (Lm <0.4):
            R = False
    elif G == True:
        if Lm > 0.65:
            R = False
        elif (not C1) and (not C2) and Lm >0.6:
            R = False
    elif  random.random() < 0.03:
        R = False
    
    
    return samples.sample(['A','B','O','C1','C2','G',
                           'L1','L2','L3','L4','R'],
                           [A,B,O,C1,C2,G,L[1],L[2],L[3],L[4],R])

outer=[]
def check(classifier,keyStates,num):
    successes = 0
    for i in range(num):
        smpl = createSample()
        for i in range(len(keyStates)):
                         
            if round(classifier.classify(smpl)[i]) != keyStates[i].extractValue(smpl):
                    #print smpl.__dict__
                    #print classifier.classify(smpl), [kstate.extractValue(smpl) for kstate in keyStates]
                    break

        else:successes +=1
    return successes

state0 = statements.get_statement(statements.op_takeValue,'A')
state1 = statements.get_statement(statements.op_takeValue,'B')
state2 = statements.get_statement(statements.op_takeValue,'O')
state3 = statements.get_statement(statements.op_takeValue,'C1')
state4 = statements.get_statement(statements.op_takeValue,'C2')
state5 = statements.get_statement(statements.op_takeValue,'G')
state6 = statements.get_statement(statements.op_takeValue,'L1')
state7 = statements.get_statement(statements.op_takeValue,'L2')
state8 = statements.get_statement(statements.op_takeValue,'L3')
state9 = statements.get_statement(statements.op_takeValue,'L4')
boolStatements = [state2,state3,state4,state5]
numStatements = [state0,state1,state6,state7,state8,state9]
keyStates =[statements.get_statement(statements.op_takeValue,'R')]#,statements.get_statement(statements.op_takeValue,'isLandDry')]

slist = [createSample_prejudiced() for i in range(100)]
numparams= ['A','B','L1','L2','L3','L4']
boolparams = ['O','C1','C2','G']
keyparams = ['R']
                                  
import time
t = time.time()

sys = system.system(keyparams,boolparams,numparams,slist,100,1000)
sys.compose()
classi = sys.tree
print time.time()-t
classi.visualise()
num = 1000
print check(classi,keyStates,num) ,'/',num
for i in range(25):
    sys.clusterise()
    sys.compose()
    sys.tree.visualise()
    print check(sys.tree,keyStates,num) ,'/',num
    for sample in [createSample() for i in range(5)]:
        sys.addSample(sample)
