using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Box2DX.Common;

namespace Car
{
    class EvoForestUnitAgent : LICSForestAgent
    {//an evo forest agent that supports cooperation via chromo transfer (EvoForestTransferAgent)
        public string groupID="";//if "", agent does not have a cooperation, else it cooperates with all compatible agents of same groupID
        public EvoForestUnitAgent(LicsDataWindow dataW, Pose initial, Vec2 target, string groupID = "")
            : base(dataW, initial, target)
        {
            this.groupID = groupID;
        }
        public int chromosGiveawayPerLoop = 3;
        //chromos for giveaway and from other ones accordingly, used with EvoForestTransferAgent
        public Queue<dynamic> chromosForExport = new Queue<dynamic>();
        public Queue<dynamic> chromosForImport = new Queue<dynamic>();
        public override void threadMethod()
        {
            system._injectedEncoder.updateSystem(system);

            while (chromosForImport.Count != 0)
            {
                dynamic chromo = chromosForImport.Dequeue();
                enc.updateChromo(chromo);// add all the same keyStatements, as they won't be added to a chromo while it's in the export queue.
                system.addChromo(chromo); // transfer agent must ensure the compatibility of encoders, so it does
            }
            var tsamples = newSamples;
            newSamples = new IronPython.Runtime.SetCollection(); //this is done so that new samples cannot affect the current loop (and cause it to crash, most likely)
            system.fullLoop(tsamples);

            foreach (dynamic chromo in system.getBestChromos(chromosGiveawayPerLoop))
                chromosForExport.Enqueue(chromo);
            
        }
    }
    class EvoForestTransferAgent:Agent
    {
        //transfers good chromos between agent systems
        List<EvoForestUnitAgent> agents;
        float sharingPeriod = 3;
        float nextSharingTime;
        dynamic enc;
        public EvoForestTransferAgent(List<EvoForestUnitAgent> agentsList)
            : base()
        {
            //warning: do not apply crossed sharing networks or on-the-fly establishment unless you are SURE they are
            //gonna use the compatible coding. This transferer does not have such property.
            agents = agentsList;
        }
        public override void initAgent()
        {
            enc = agents.First().enc;
            foreach (var agent in agents)
            {//forces the same encoding
                agent.system._injectedEncoder = enc;
                agent.enc = enc;
            }

        }
        public override Control react(PerceptParams pp, float t)
        {
            if (t > nextSharingTime)
            {
                nextSharingTime += sharingPeriod;
                foreach (var sender in agents)
                {
                    while (sender.chromosForExport.Count != 0)
                    {
                        dynamic chromo = sender.chromosForExport.Dequeue();
                        foreach (var receiver in agents)
                        {
                            if (sender != receiver)
                                receiver.chromosForImport.Enqueue(chromo);//or copy?

                        }
                    }
                }
            }
            return new Control{};
        }

    }
}
