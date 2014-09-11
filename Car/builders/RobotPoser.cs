using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Box2DX.Common;

namespace Car.builders
{
    partial class RobotPoser : Builder
    {
        Environment env;
        Physics p;
        Form1 form;
        
        public RobotPoser(Form1 form)
        {
            InitializeComponent();
            env = form.env;
            p = form.env.physics;
            this.form = form;
            foreach (string agentname in form.agents.Keys)
            {
                robotListBox.Items.Add(agentname);
            }
        }

        public override void reportClick(float x, float y)//перемещает робота в место клика
        {
            try
            {

                string selectedName = robotListBox.SelectedItem as string;
                if (selectedName == null)
                {
                    MessageBox.Show("No agent selected");
                    return;
                }

                Agent activeAgent = form.agents[selectedName];

                if (radioButton1.Checked)
                {
                    env.robotsDict[activeAgent].Move(new Pose { xc = x / 10, yc = y / 10, angle_rad = Convert.ToSingle(textBox1.Text) });
                }
                else
                {
                    env.targetsDict[activeAgent] = new Box2DX.Common.Vec2 { X = x / 10, Y = y / 10 };
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to commit action\n"+e.Message);
            }
            
        }

        
        private void removeBtn_Click(object sender, EventArgs e)
        {
            string activeAgentName = robotListBox.SelectedItem as string;
            if (activeAgentName == null)
            {
                MessageBox.Show("no agent selected");
                return;
            }
            Agent activeAgent = form.agents[activeAgentName];
            env.removeRobot(activeAgent);
            form.agents.Remove(activeAgentName);
            robotListBox.Items.Remove(activeAgentName);
#warning debug window is NOT removed until restart, yet they are never shown.



        }

        private void robotListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

            string name = robotListBox.SelectedItem as string;
            if (name == null)
                if (robotListBox.Items.Count != 0)
                    robotListBox.SelectedIndex = 0;

            if (name != null)
            {
                TypeListBox.SelectedItem = form.typeOfAgent(form.agents[name]);

                var group = "";
                if (form.typeOfAgent(form.agents[name]) == "LICS forest car agent with transfer")
                    group = (form.agents[name] as EvoForestUnitAgent).groupID;
                groupIDbox.Text = group;
             
                nameBox.Text = name;
            }
        }
        Agent addNewAgent(string name, string type, string groupID = "", int index = 0)
        {
            Agent agent = form.addNewAgent(name, type, groupID);
            if (agent == null) return null;
            
            robotListBox.Items.Insert(index,name);
            
            robotListBox.SelectedItem = name;
            TypeListBox.SelectedItem = type;
            nameBox.Text = name;
            groupIDbox.Text = groupID;
            return agent;
        }
        private void addButton_Click(object sender, EventArgs e)
        {
            string type = TypeListBox.SelectedItem as string;
            if (type == null)
                type = TypeListBox.Items[0] as string;
            string name;
            for (int i = 0; ; i++)
            {
                name = type + " " + i.ToString();
                if (addNewAgent(name, type,"1")!= null)
                    break;
            }

        }

        

        private void confirmEditButton_pressed(object sender, EventArgs e)
        {
            string newName = nameBox.Text;
            string selectedName = robotListBox.SelectedItem as string;
            string activeAgentType = TypeListBox.SelectedItem as string;
            string groupName = groupIDbox.Text;
            //why?!: it's only safe to save these strings now, as the list index change event handler procedure nulls them after 
            //you remove the old agent at least on some machines.

            if (selectedName == null)
            {
                MessageBox.Show("No agent selected");
                return;
            }

            if (newName == "" || (selectedName != newName && form.agents.ContainsKey(newName)))
            {
                MessageBox.Show("The suggested name is incorrect");
                return;
            }


            Agent activeAgent = form.agents[selectedName];
            Box2DX.Common.Vec2 targ = Box2DX.Common.Vec2.Zero;
            Pose pos = new Pose { };
            if (env.robotsDict.ContainsKey(activeAgent))
            {
                pos = env.robotsDict[activeAgent].pos;
                targ = env.targetsDict[activeAgent];
            }
            env.removeRobot(activeAgent);
            form.agents.Remove(selectedName);


            
            activeAgent = addNewAgent(newName, activeAgentType,groupName,robotListBox.Items.IndexOf(selectedName));
            robotListBox.Items.Remove(selectedName);

            if (targ != Vec2.Zero && activeAgentType != "LICS forest coordinator")
            {
                env.targetsDict[activeAgent] = targ;
                env.robotsDict[activeAgent].Move(pos);

            }


        }



    }
}
