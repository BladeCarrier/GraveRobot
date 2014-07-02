import statements
#import copy


class connectionRecord:
    '''everything for a link between 2 statements'''
    def __init__ (self,state1, state2, bothTrue=0, bothFalse=0, trueFalse=0, falseTrue=0,
                  normalizerPositive=0,normalizerNegative = 0):
        self.statement1 = state1
        self.statement2 = state2
        self.bothTrue = bothTrue
        self.bothFalse = bothFalse
        self.falseTrue = falseTrue
        self.trueFalse = trueFalse
        self.normalizerPositive = float(normalizerPositive)
        self.normalizerNegative = float(normalizerNegative)

        self.strength = 0.0
    def update(self,sample,calculateAfterwards = True):
        '''process a sample'''
        w = sample._weight
        if self.statement2.extractValue(sample) == True:
            if self.statement1.extractValue(sample) == True:
                self.bothTrue +=w
                self.normalizerPositive +=w
            elif self.statement1.extractValue(sample) == False:
                self.falseTrue +=w
                self.normalizerPositive +=w
            else:raise ValueError, 'Non-boolean value!'
            
        elif self.statement2.extractValue(sample) == False:
            if self.statement1.extractValue(sample) == True:
                self.trueFalse +=w
                self.normalizerNegative +=w
            elif self.statement1.extractValue(sample) == False:
                self.bothFalse +=w
                self.normalizerNegative+=w
            else: raise ValueError, 'Non-boolean value!'
        else: raise ValueError, 'Non-boolean value!'
        if calculateAfterwards: self.calculate()
    def calculate(self):
        '''returns the total strength(sum/2) of a connection'''
        rslt = (self.getPositiveStrength()+ self.getNegativeStrength())/2
        self.strength = rslt     
        return rslt
    def getPositiveStrength(self):
        ''' bothTrue - falseTrue, normalized'''
        if self.normalizerPositive !=0: return (self.bothTrue - self.falseTrue)/ (self.normalizerPositive)
        return 0.0
    def getNegativeStrength(self):
        '''bothFalse - trueFalse, normalized'''
        if self.normalizerNegative !=0: return (self.bothFalse - self.trueFalse)/(self.normalizerNegative)
        return 0.0
    def processSamples(self,samples):
        '''processes the selected list of samples'''
        for smpl in samples: self.update(smpl)
        
        
        
        
        
        

def getConnectionStrength(st1,st2,smpls):
    '''returns the strength of connection'''
    cr = connectionRecord(st1,st2)
    cr.processSamples(smpls)
    return cr.strength

class candidateRecord:
    def __init__(self,keyStates,valueState,openLinks,samples):
        self.keyStates = keyStates #a connection record for every state
        self.valueState = valueState
        self.used = [valueState]
        self.openLinks = openLinks
        self.strength =0.
        self.divisingState =None #output statement like a>const
        self.recs = None #records with all keystates
        for keyState in keyStates:
            # optimisable!
            stmnt = getBestLinkOverDiap(self.valueState,keyState,samples,False)
            recs = [connectionRecord(stmnt,kstate) for kstate in keyStates]
            for rec in recs: rec.processSamples(samples)
            newstr = sum(abs(cstrec.strength) for cstrec in recs)
            if newstr > self.strength:
                self.strength = newstr
                self.divisingState = stmnt
                self.recs = recs
    def getStrength(self):return self.strength

def getBestLink(keyStates,samples,boolStates,numStates=[], needRecords= False,intelligent = True):
    '''returns the best link - statement or record.'''
    #if len(boolStates) ==0:
    #    boolStates.append(statements.get_statement(statements.op_takeConstant,[True]))
    bflag = False
    maxval =0.0;
    states = boolStates
    bestRecords = None
    for stmnt in states:
        cstrecs = [connectionRecord(stmnt,keystate) for keystate in keyStates]
        for cstrec in cstrecs: cstrec.processSamples(samples)
        strength = sum(abs(cstrec.strength) for cstrec in cstrecs)
        if strength >= maxval:
            maxval = strength
            bestRecords = cstrecs
    if numStates != []:
        if not intelligent:
            numRecordsets = [getBestLinkOverDiap(nstate,keystate,samples,True) for keystate in keyStates for nstate in numStates]

            for numRec in numRecordsets:
                cstrecs = [connectionRecord(numRec.statement1,keystate) for keystate in keyStates]
                for cstrec in cstrecs: cstrec.processSamples(samples)
                strength = sum(abs(cstrec.strength) for cstrec in cstrecs)
                if strength >= maxval:
                    maxval = strength
                    bestRecords = cstrecs
                    
        else:
            candidates = [candidateRecord (keyStates, nstate,[],samples)
                                        for nstate in numStates]
                

            for i in range(len(candidates)):
                candidates[i].openLinks = candidates[:i]
                
            strengths = [cand.getStrength() for cand in candidates]
            bestCand = candidates[strengths.index(max(strengths))]
            while len(candidates) != 0:
                maxind = strengths.index(max(strengths))
                current = candidates.pop(maxind)
                
                if abs(current.getStrength()) >= abs(bestCand.getStrength()):
                    bestCand = current
                    if abs(bestCand.getStrength()) ==1: break
                del strengths[maxind]
                
                for link in current.openLinks:
                    
                    if any((u in current.used) for u in link.used):continue #mind explainin...?
                        
                    newState = statements.get_sum([current.valueState,link.valueState])
                    newCand = candidateRecord(keyStates,newState,candidates,samples)
                    newStrength = newCand.getStrength()
                    if newStrength > current.getStrength() and newStrength > link.getStrength():
                        newCand.used = current.used + link.used
                        candidates+=[newCand]
                        strengths +=[newCand.getStrength()]
                    #difference
                    newState = statements.get_sum([current.valueState,statements.get_minus(link.valueState)])
                    newCand = candidateRecord(keyStates,newState,candidates,samples)
                    newStrength = newCand.getStrength()
                    if newStrength > current.getStrength() and newStrength > link.getStrength():
                        newCand.used = current.used + link.used
                        candidates+=[newCand]
                        strengths +=[newCand.getStrength()]
                        bflag = True

                        
            if abs(bestCand.getStrength() ) >= abs(maxval):
                bestRecords = bestCand.recs
                maxval = bestCand.getStrength()
    
    if bestRecords == None: 
        return False
        raise ArithmeticError, 'ok'+ str(maxval) + str(bflag) + str(bestCand.valueState.toString())

    return bestRecords if needRecords else bestRecords[0].statement1

      
      
      
def getBestLinkOverDiap(valueStatement,statement2, samples,needRecord = False):
    '''returns the best moreThan statement (valueStatement more than const) to classify statement2 in samples list'''
    valsSorted = []
    valdict = {}
    secondBoolVals = []


    for sample in samples:
        value = valueStatement.extractValue(sample)
        valdict[value] = sample
        for i in range( len(valsSorted) ):
            if value < valsSorted[i]:
                valsSorted.insert(i,value)
                secondBoolVals.insert(i,statement2.extractValue(sample))
                break
        else: 
            valsSorted.append(value)
            secondBoolVals.append(statement2.extractValue(sample))

    initial = connectionRecord(statements.get_moreThan(valueStatement,valsSorted[0]-0.1),statement2)
    initial.processSamples(samples)
    both_true = both_true_best = initial.bothTrue
    both_false = both_false_best = initial.bothFalse
    trueFalse = trueFalse_best = initial.trueFalse
    falseTrue = falseTrue_best = initial.falseTrue
    normalizerPositive = initial.normalizerPositive
    normalizerNegative = initial.normalizerNegative
    def getCurStrength():
        '''internal function -> simplified getConnectionStrength'''
        rslt =0.0
        if normalizerPositive != 0: rslt +=(both_true - falseTrue)/(2.0*normalizerPositive)
        if normalizerNegative != 0: rslt+= (both_false - trueFalse )/(2.0*normalizerNegative)
        return rslt        

    bestStrength =  getCurStrength()
    bestValueIndex = -1


    for i in range(len(valsSorted)):
        w = valdict[valsSorted[i]]._weight
        if secondBoolVals[i] == True:
            both_true -=w
            falseTrue +=w
        else: #st2 gives false
            both_false +=w
            trueFalse -=w
        if i == len(valsSorted)-1 or valsSorted[i] != valsSorted[i+1]:                
            strength =getCurStrength()
            if abs(strength ) > abs(bestStrength):
                bestStrength = strength
                bestValueIndex = i
                both_true_best = both_true
                both_false_best = both_false
                falseTrue_best = falseTrue
                trueFalse_best = trueFalse

    bestVal = valsSorted[bestValueIndex]
    if bestValueIndex != len(valsSorted)-1 and bestValueIndex !=-1:
        bestVal = 0.5*bestVal + 0.5*valsSorted[bestValueIndex+1]
    bestStatement = statements.get_moreThan(valueStatement,bestVal)
    if bestStrength <0:
        bestStatement = statements.get_negation(bestStatement)

    if needRecord:
        if bestStrength >=0:
            rec = connectionRecord(bestStatement,statement2,both_true_best,both_false_best,trueFalse_best,falseTrue_best,
                             normalizerPositive,normalizerNegative)
        else:
            rec = connectionRecord(bestStatement,statement2,falseTrue_best,trueFalse_best,both_false_best,both_true_best,
                             normalizerPositive,normalizerNegative)
        rec.strength = abs(bestStrength)
        return rec
    
    return bestStatement
                
    
