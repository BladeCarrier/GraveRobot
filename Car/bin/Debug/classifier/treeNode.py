# -*- coding: utf-8 -*-
"""
Created on Mon Oct 28 21:42:50 2013

@author: sasha_000
"""
import helpers
class treeNode:
    def __init__(self,samples,keyStatements,majorant = False):
        '''a recursive decision tree class'''
        self.isTerminal = True
        self.isMajorant = majorant
        self.dichotomy = None
        self.samples = samples
        self.keyStatements = keyStatements
        self.result = [0 for i in keyStatements]
        self.entropy=0.
        self.updated = True
    def expand(self,dichotomy):
        '''extend the classification by spliting this node with a given dichotomy'''
        self.dichotomy = dichotomy
        posSamples = set()
        negSamples = set()
        for sample in self.samples:
            if dichotomy.extractValue(sample):
                posSamples.add(sample)
            else:
                negSamples.add(sample)
        self.childPositive = treeNode(posSamples,self.keyStatements,self.isMajorant)
        self.childNegative = treeNode(negSamples,self.keyStatements,self.isMajorant)
        self.isTerminal = False
    def classify(self,sample,rnd = False):
        '''Classify a sample according to this classification's rules'''
        if not self.isTerminal:
            cls = self.dichotomy.extractValue(sample)
            if cls: rslt = self.childPositive.classify(sample)
            else: rslt = self.childNegative.classify(sample)
        else:
            rslt = self.result
        if rnd:
            return [int(round(r)) for r in rslt]
        else:
            return rslt
            
    def addSample(self,sample):
        self.samples.add(sample)
        self.updated = False
        if not self.isTerminal:
            if self.dichotomy.extractValue(sample):
                self.childPositive.addSample(sample)
            else:
                self.childNegative.addSample(sample)
    def update(self,onlyResult = False):
        '''updates result and, if asked, the entropy of a node'''
        if not onlyResult:
            self.entropy = helpers.getBoolEntropy(self.samples,self.keyStatements)            
        if not self.isMajorant:
            fchoose = helpers.getAverage
        else:
            fchoose = helpers.getMajorant

        self.result = fchoose(self.keyStatements,self.samples)
        self.updated = True
    
    def getInformationGain(self):
        '''information gain of a given dichotomy for the last update'''
        assert (not self.isTerminal)
        return self.entropy - self.childPositive.entropy*len(self.childPositive.samples)*1./len(self.samples) - self.childNegative.entropy*len(self.childNegative.samples)*1./len(self.samples)
    def getReplacementPotential(self):
        '''how useful is it to start a new classi from this node including it's redichotomisation'''
        ###gotta think it through!
        v = len(self.samples)
        if self.isTerminal:
            v *= helpers.getBoolEntropy(self.samples,self.keyStatements)
        else:
            v *= helpers.getInformationGain(self.samples,self.dichotomy,self.keyStatements)- self.getInformationGain(self)
        return v
    def visualise(self,encoder = None):
        classi = self
        if self.isTerminal:
            return ""
        resString = ""
        classi.depth = 1
        openList = [classi.childNegative,classi.childPositive]
        resString+=( classi.depth*2*' '+'IF'+ classi.dichotomy.toString().replace('op_','')+':'+'\n') #str(classi.result)+ was there before ':' until 5.04.2014
        classi.childPositive.depth =2
        classi.childPositive.pos = True
        classi.childNegative.depth =2
        classi.childNegative.pos = False
        while len(openList) !=0:
             cur =openList.pop(len(openList)-1)
             if cur.pos:
                 prefix = 'THAN '
             else:
                 prefix = 'ELSE '
             if  not cur.isTerminal:
                 statement = cur.dichotomy.toString()
                 resString+= (cur.depth*2*' '+prefix+'IF'+ statement.replace('op_','')+':'+'\n') #until 5.4.2014 there was +str(cur.result) before +':'
                 cur.childNegative.depth = cur.depth+1
                 cur.childPositive.pos = True
                 cur.childPositive.depth = cur.depth+1
                 cur.childNegative.pos = False
                 openList.append(cur.childNegative)
                 openList.append(cur.childPositive)
             else:
                 res = cur.result
                 if encoder != None:
                    try: 
                         res = encoder.decode(res)
                    except :pass
                 resString+= (cur.depth*2*' '+prefix+'result ='+str(res)+'\n')
        return resString



