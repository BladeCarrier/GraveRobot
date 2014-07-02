# -*- coding: utf-8 -*-
"""
Created on Tue Oct 29 21:51:23 2013

@author: sasha_000
"""

from classifier.treeNode import treeNode
def getTree(boolStatements,numStatements,keyStatements,samples,nodesCount,optimisation=False,deadline = 0.005,isMajorant = False):
    '''dijkstra-inspired classifier'''
    #expand in order of replacement potential descension
    if not (keyStatements is list): keyStatements = list(keyStatements)
    if not (samples is set): samples = set(samples)
    treeRoot = treeNode(samples, keyStatements,isMajorant)
    treeRoot.update()
    openList = [treeRoot]
    budget = nodesCount
    while budget > 0 :
        budget -=2
        if openList ==[]: break
        current = openList.pop(0) #must be sorted
        divisor = factor_connection.getBestLink(keyStatements,current.samples,boolStatements,numStatements,False,True)
        if divisor == False: continue;##this was absent in the original build.

        expand(current,boolStatements, numStatements,keyStatements,divisor)
        newNodes = []
        if current.getInformationGain() < deadline:
            continue
        if current.childPositive.entropy >= deadline:
            newNodes.append(current.childPositive)
        if current.childNegative.entropy >= deadline:
            newNodes.append(current.childNegative)
        for node in newNodes:
            repPotential = node.getReplacementPotential()
            for cmpInd  in range(len(openList)):
                if repPotential > openList[cmpInd].getReplacementPotential():
                    openList.insert(cmpInd,node)
                    break
            else:
                openList.append(node)
        
    if optimisation:
        treeRoot.optimize()
    return treeRoot

import factor_connection
def expand(current,boolStatements, numStatements, keyStatements,divisor):
    current.expand(divisor)
    current.childPositive.update()
    current.childNegative.update()
