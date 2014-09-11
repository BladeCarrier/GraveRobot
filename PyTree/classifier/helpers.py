# -*- coding: utf-8 -*-
#some useful, but not major functions

def getSize(classification):
    '''returns the amount of nodes in the current the tree classification'''
    olist = [classification]
    depth = 0
    while olist !=[]:
        cur = olist.pop(0)
        depth +=1
        if cur.isExtended: 
            olist.append(cur.childPositive)
            olist.append(cur.childNegative)
    return depth
def getMajorant(states, samples, smoothing = 0):
    #majorant vector used to generate result vector for keyStatements
    if len(samples) ==0:
        return [0 for i in states]
    options = [tuple(statement.extractValue(sample) for statement in states) for sample in samples]
    maxOpt = None
    maxVal = -float('inf')
    for option in set(options):
        val = options.count(option)
        if val>maxVal:
            maxVal = val
            maxOpt = option
    assert maxOpt != None  
    return list(maxOpt)
        
        

def getAverage(states, samples, smoothing = 0):
    #list of averages used to generate result vector for keyStatements
    return [getProbability(statement,samples,smoothing) for statement in states]
def getProbability (state, samples,smoothing = 0):
    cnt = 0. + smoothing
    for sample in samples:
        if state.extractValue(sample):
            cnt +=sample._weight
    if cnt ==0: return 0.
    return cnt/(sum(i._weight for i in samples) + 2*smoothing)

def getBoolEntropy(samples,keyStatements, b=2):
    import math
    ent = 0.
    outcomes = [0. for i in range(2**len(keyStatements))]
    norm = sum(i._weight for i in samples)+0.
    for sample in samples:
        outcomeID = 0
        for i in range(len(keyStatements)):
            if keyStatements[i].extractValue(sample):
                outcomeID += 2**i
        outcomes[outcomeID] += sample._weight
    
    for outcome in outcomes:
        if outcome ==0:continue
        p = outcome/norm
        if p ==0 or norm ==0 : continue #lim(p->0)p * log p  = 0
        ent -= p * math.log(p,b)
    return ent

def getInformationGain(samples,dichotomy, keyStatements, b = 2):
    samplesPos = []
    samplesNeg = []
    for sample in samples:
        if dichotomy.extractValue(sample):
            samplesPos.append(sample)
        else:
            samplesNeg.append(sample)
    return getBoolEntropy(samples,keyStatements,b) - getBoolEntropy(samplesPos,keyStatements,b)*len(samplesPos)*1./len(samples) - getBoolEntropy(samplesNeg,keyStatements,b)*len(samplesNeg)*1./len(samples)
        

def getOpposites(numStatements):
    '''list of -1*numState statements'''
    import statements
    opposites = []
    for statement in numStatements:
        opposites.append(statements.getStatement(statements.op_minus,[statement]))
    return opposites
def getPairedSums(numStatements):
    import statements
    numCombinations = []
    for statement1 in numStatements:
        for statement2 in numStatements:
            if statement1 == statement2:continue
            numCombinations.append(statements.get_statement(statements.op_sum,[statement1,statement2]))
    return numCombinations
def getDifferences(numStatements):
    import statements
    numCombinations = []
    for i in range(len(numStatements)):
        for j in range(i,len(numStatements)):
            numCombinations.append(statements.get_statement(statements.op_sum,
                                                            [statements[i],
                                                             statements.get_statement(statements.op_minus,[statements[j]])]))
    return numCombinations
def getCombinations(numStatements):
    import statements
    combinations = []
    for code in range(1,3**len(numStatements)):
        s = statements.statement()
        s.operation = statements.op_sum
        s.args = []
        minusCount = 0
        plusCount = 0
        for i in range(len(numStatements)):
            form = (code % (3**(i+1)))/(3**i)
            if form ==0: continue
            elif form ==1:
                plusCount +=1
                s.args.append(numStatements[i])
            else: #form ==2
                minusCount +=1
                s.args.append(statements.get_statement(statements.op_minus,numStatements[i]))
        if plusCount >= minusCount: 
            combinations.append(s)
            
            
    return combinations
def visualize(classi):
    '''displays the nodes of the classification given'''
    classi.depth = 1
    openList = [classi]
    while len(openList) !=0:
         cur =openList.pop(0)
         if cur.isExtended:
             print cur.depth, cur.classifier.toString(),cur.probability
             cur.childNegative.depth = cur.depth+1
             cur.childPositive.depth = cur.depth+1
             openList.append(cur.childNegative)
             openList.append(cur.childPositive)
         else: print cur.depth,'[end]',cur.probability
    