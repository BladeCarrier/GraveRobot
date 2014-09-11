# -*- coding: utf-8 -*-
"""
Created on Fri Sep 27 09:34:57 2013

@author: sasha_000
"""
class RandomPy:
    def __init__(self):
        import clr
        clr.AddReferenceByPartialName("System")
        from System import Random
        self.r = Random()
    def choice(self,arr):
        return arr[self.r.Next %len(arr)]
    
random = RandomPy()
from classifier import samples
def copySample(sample, attrs):
    vals = []
    for attr in attrs:
        vals.append(getattr(sample,attr))
    return samples.sample(attrs,vals)
'''might be useful to build a cluster tree to simplify grouping'''

'''second version MUST include param weights from the classification and nonlinearity(f(x)) in BOTH distance evaluation AND centralisation'''

def lambda_aux(k,b=0):
    '''this function generates a function f(x) = lamb(x)*k, which would have been generated incorrectly due to python bug in multiple lambda creation inside single function'''
    return lambda x: x*k+b
    
class kmeans:
    '''the first clusterisation module; works iteratively as grasp-clusterise-replace, does not support on-fle-fly samples addition'''
    def __init__(self,sys, numattrs, boolattrs,  clustsize = None,weightdecay=0.9):
        self.system = sys
        self.targetclustsize = clustsize
        self.strictboolattrs = boolattrs
        self.clustergroup = dict()
        self.attrfunctions = {}
        for attr in boolattrs+numattrs:
            self.attrfunctions[attr]=lambda x:x
        self.weightdecay= weightdecay
    def grasp(self):
        ini = list(self.system.tree.samples)
        if self.targetclustsize == None or self.targetclustsize >= len(ini) or self.targetclustsize <=0:
            self.clustsize = len(ini)/2
        else:
            self.clustsize = self.targetclustsize
        self.clustergroup = dict()
        for i in range(self.clustsize):
            clust = random.choice(ini)
            self.clustergroup[copySample(clust,self.attrfunctions.keys)]= set()
            ini.remove(clust)
        self.samples = set(self.system.tree.samples)
    def normalise(self):
        mins = {}
        for attr in self.attrfunctions:
            mins[attr] = float('inf')
        maxes= {}
        for attr in self.attrfunctions:
            maxes[attr] = -float('inf')
        for smpl in self.samples:
            for attr in self.attrfunctions:
                val = getattr(smpl,attr)
                if val> maxes[attr]:
                    maxes[attr] = val
                if val< mins[attr]:
                    mins[attr] = val
        for attr in self.attrfunctions:
            if mins[attr] == maxes[attr]: 
                self.attrfunctions[attr] = lambda_aux(0)
            else:
                self.attrfunctions[attr] = lambda_aux(1./(maxes[attr]-mins[attr]))

    def getDistance(self,s1,s2,squared = False,squareLessThan = float('inf')):
        '''returns a distance between two samples. If lessThanSquare is set to a number, the calculation is optimised, but it is allowed to return infinity if exceeding the maximum'''
        s = 0.
        for attr in self.attrfunctions:
            s+=(self.attrfunctions[attr](getattr(s1,attr)) - self.attrfunctions[attr](getattr(s2,attr)))**2
            if s>= squareLessThan:
                return float('inf')
        if not squared:
            s=s**0.5
        return s
        
        
    def assign(self):
        #axis: for every parameter, the clusters are sorted/arranged into an axis. If a cluster from a sample is to the opposite(or equal) side of current best cluster, it's ignored
        for cluster in self.clustergroup:
            self.clustergroup[cluster] = set()
        for sample in self.samples:
            dist = float('inf')
            clu = None
            for cluster in self.clustergroup:
                di = self.getDistance(sample,cluster,True,dist)
                if di <dist:
                    dist = di
                    clu = cluster
            assert(clu != None)
            self.clustergroup[clu].add(sample)
            sample.__cluster = clu
            
    def centralise(self, calcdist = False):
        sumdist = 0
        for cluster in self.clustergroup:
            group = self.clustergroup[cluster]
            if len(group) == 0: continue #it can't be empty in classic kmeans, can it? it can.
            divs = [0]
            for attr in self.attrfunctions:
                newattr = (sum(getattr(smpl,attr)*smpl._weight for smpl in group))*1./sum(i._weight for i in group)
                if calcdist:
                    divs.append(newattr - getattr(cluster,attr))
                setattr(cluster, attr, newattr)
            if calcdist:
                sumdist += sum(d**2 for d in divs)**0.5
        if calcdist:
            return sumdist
            
            
                
    def replace(self):
        classiNodes = [self.system.tree]
        for cluster in self.clustergroup:
            cluster._weight = sum(s._weight for s in self.clustergroup[cluster])*self.weightdecay
            for attr in self.strictboolattrs:
                setattr(cluster,attr, bool(round(getattr(cluster,attr))))
        while(len(classiNodes)!=0):
            node = classiNodes.pop(0)
            if not node.isTerminal:
                classiNodes.append(node.childPositive)
                classiNodes.append(node.childNegative)
                
            #tiss' enumerating allda nodes
            newsamples = set()
            for sample in node.samples:
                if sample not in self.samples:
                    newsamples.add(sample)
                else:
                    newsamples.add(sample.__cluster)
                    
            node.samples = list(newsamples)
        for sample in self.samples:
            del sample.__cluster
        self.system.samples = self.clustergroup.keys()
    def run(self,iterations = float('inf'),tolerance = 0.000000001):
        self.grasp()
        if self.clustsize < self.targetclustsize:
            return;
        self.normalise()
        i=0
        dist = float('inf')
        while i<iterations and dist >tolerance:
            i+=1
            self.assign()
            dist = self.centralise(True)
        self.replace()
        
