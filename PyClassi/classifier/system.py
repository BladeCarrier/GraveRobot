# -*- coding: utf-8 -*-
"""
Created on Mon Oct 28 22:43:23 2013

@author: sasha_000
"""
#it was noticed, that first classification takes much time on calculating numeric samples.
#it is useful to keep the candidates from the search.
import treeNode
import learning
import helpers
import cluster
import statements
class system:
    def __init__(self,keyParams,boolParams, numParams, samples, samplesCount, nodesCount, majorant = False):
        '''the LICS itself'''
        '''reverse keyparams pls!'''
        self.boolParams = boolParams
        self.numParams = numParams
        self.keyParams = keyParams
        self.keyStatements = [ statements.get_statement(statements.op_takeValue,p) for p in keyParams]
        self.boolStatements = [ statements.get_statement(statements.op_takeValue,p) for p in boolParams]
        self.numStatements = [ statements.get_statement(statements.op_takeValue,p) for p in numParams]
        self.samples = set(samples)
        self.samplesCount = samplesCount
        self.nodesCount = nodesCount
        self.tree = treeNode.treeNode(self.samples,self.keyStatements,majorant)
        self.clusteriser = cluster.kmeans(self,numParams,boolParams + keyParams,samplesCount)
        self.isMajorant = majorant
    def addSample(self,sample):
        '''add a sample into the samples list and into the tree structure'''
        self.samples.add(sample)
        #self.tree.addSample(sample)
    def compose(self,params = None):
        '''build the decision tree from the ground'''
        self.tree = learning.getTree(self.boolStatements,self.numStatements,self.keyStatements,set(list(self.samples)),self.nodesCount,isMajorant =self.isMajorant)
        
    def update(self):
        ''' alter the decision tree in accordance to the new samples given'''
        nodes = [self.tree]
        bestRePo = self.tree.getReplacementPotential()
        bestNode = self.tree
        bestParent = None
        isPositive = None
        for node in nodes:
            if node.isTerminal:
                continue
            nodes.append(node.childNegative)
            nodes.append(node.childPositive)
            nn = node.childNegative
            potential = nn.getReplacementPotential()
            if potential > bestRePo:
                bestRePo = potential
                bestNode = nn
                bestParent = node
                isPositive = False
            nn = node.childPositive
            potential = nn.getReplacementPotential()
            if potential > bestRePo:
                bestRePo = potential
                bestNode = nn
                bestParent = node
                isPositive = True
        budget = self.nodesCount - helpers.getSize( self.tree) + helpers.getSize(bestNode)
        newTree = learning.getTree(self.boolStatements,self.numStatements,self.keyStatements,bestNode.samples,budget)
        if bestParent ==None:
            self.tree = newTree
        elif isPositive:
            bestParent.childPositive = newTree
        else:
            bestParent.childNegative =  newTree
    def clusterise(self):
        self.clusteriser.run(tolerance = 0.0001)
        
        
        