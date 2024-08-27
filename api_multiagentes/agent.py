import agentpy as ap
import pathfinding as pf
import matplotlib.pyplot as plt
from owlready2 import *
import itertools
import random
import IPython
import math

from constants import positions


onto = get_ontology("file://onto.owl")

onto.destroy(update_relation = True, update_is_a = True)

with onto:
    class Entity(Thing):
      pass

    class Robot(Entity):
      pass

    class Box(Entity):
      pass

    class Goal(Entity):
      pass

    class Place(Thing):
      pass

    class Position(Thing):
      pass

    class is_in_place(ObjectProperty):
      domain = [Entity]
      range = [Place]
      pass

    class has_position(ObjectProperty, FunctionalProperty):
      domain = [Place]
      range = [str]
      pass

    class box_within_reach(ObjectProperty):
      domain = [Robot]
      range = [int]

    class goal_within_reach(ObjectProperty):
      domain = [Robot]
      range = [int]

class RobotAgent(ap.Agent):

    def see(self, e):
        seeRange = 10
        P = [a for a in e.neighbors(self, distance=seeRange) if a.agentType == 1]
        return P

    def see_second(self,e):
      seeRange = 10
      G = [a for a in e.neighbors(self,distance=seeRange) if a.agentType==2 and a.pile<=5]
      return G

    def brf(self, p):
        """
        It will update the Belief system of the agent.
        The belief system is based on the Ontology.
        """

        # Destroys previous beliefs
        for box in self.this_robot.box_within_reach:
            destroy_entity(box.is_in_place[0])
            destroy_entity(box)
        destroy_entity(self.this_robot.is_in_place[0])

        # Ontologically instantiate the robot
        currentPos = self.model.Store.positions[self]
        self.this_robot.is_in_place = [Place(at_position=str(currentPos))]

        # Ontologically instantiate the objects at reach
        for b in p:
            theBox = Box(is_in_place=[Place()])
            theBox.is_in_place[0].at_position = str(self.model.Store.positions[b])
            self.this_robot.box_within_reach.append(theBox)

    def brf_second(self, g):

      for goal in self.this_robot.goal_within_reach:
        destroy_entity(goal.is_in_place[0])
        destroy_entity(goal)
      destroy_entity(self.this_robot.is_in_place[0])

      currentPos = self.model.Store.positions[self]
      self.this_robot.is_in_place = [Place(at_position=str(currentPos))]

      for b in g:
        theGoal = Goal(is_in_place=[Place()])
        theGoal.is_in_place[0].at_position = str(self.model.Store.positions[b])
        self.this_robot.goal_within_reach.append(theGoal)


    def options(self):
        """Returns the available goals to pursue based on the distance of each object relative to the robot."""
        distances = {}

        for onto_box in self.this_robot.box_within_reach:
            box_pos = eval(onto_box.is_in_place[0].at_position)
            robot_pos = eval(self.this_robot.is_in_place[0].at_position)
            d = math.sqrt((box_pos[0] - robot_pos[0]) ** 2 + (box_pos[1] - robot_pos[1]) ** 2)
            distances[onto_box] = d

        return distances

    def options_second(self):

      distances_to_goals = {}

      for onto_goal in self.this_robot.goal_within_reach:
        goal_pos = eval(onto_goal.is_in_place[0].at_position)
        robot_pos = eval(self.this_robot.is_in_place[0].at_position)
        d = math.sqrt((goal_pos[0] - robot_pos[0]) ** 2 + (goal_pos[1] - robot_pos[1]) ** 2)
        distances_to_goals[onto_goal] = d

      return distances_to_goals

    def filter(self):
        desires = {x: y for x, y in sorted(self.D.items(), key=lambda item: item[1])}
        return list(desires.items())[0][0] if desires else None


    def filter_second(self):
      desires = {x: y for x, y in sorted(self.D.items(), key=lambda item: item[1])}
      return list(desires.items())[0][0] if desires else None


    def plan(self):
        """Creates a plan towards the current Intention."""
        if self.I is None:
            if random.randint(0, 1) == 0:
                return [(random.choice([-1, 1]), 0)]
            else:
                return [(0, random.choice([-1, 1]))]

        thePlanX = []
        thePlanY = []

        boxPos = eval(self.I.is_in_place[0].at_position)
        robotPos = eval(self.this_robot.is_in_place[0].at_position)
        distance2D = (boxPos[0] - robotPos[0], boxPos[1] - robotPos[1])

        for i in range(abs(distance2D[0])):
            thePlanX.append(1 if distance2D[0] >= 0 else -1)

        for j in range(abs(distance2D[1])):
            thePlanY.append(1 if distance2D[1] >= 0 else -1)

        thePlanX = list(zip(thePlanX, [0] * len(thePlanX)))
        thePlanY = list(zip([0] * len(thePlanY), thePlanY))

        thePlan = thePlanX + thePlanY

        return thePlan

    def plan_second(self):

      if self.I is None:
        if random.randint(0, 1) == 0:
          return [(random.choice([-1, 1]), 0)]
        else:
          return [(0, random.choice([-1, 1]))]

      thePlanX = []
      thePlanY = []

      goalPos = eval(self.I.is_in_place[0].at_position)
      robotPos = eval(self.this_robot.is_in_place[0].at_position)
      distance2D = (goalPos[0] - robotPos[0], goalPos[1] - robotPos[1])

      for i in range(abs(distance2D[0])):
          thePlanX.append(1 if distance2D[0] >= 0 else -1)

      for j in range(abs(distance2D[1])):
          thePlanY.append(1 if distance2D[1] >= 0 else -1)

      thePlanX = list(zip(thePlanX, [0] * len(thePlanX)))
      thePlanY = list(zip([0] * len(thePlanY), thePlanY))

      thePlantoGoal = thePlanX + thePlanY
      # print(thePlantoGoal)

      return thePlantoGoal




    def BDI(self, p):
        """Calls all functions from the BDI architecture."""

        self.brf(p)
        if self.intentionSucceded:
            self.intentionSucceded = False
            self.D = self.options()
            self.I = self.filter()
            self.currentPlan = self.plan()


    def BDI_second(self, g):
      self.brf_second(g)
      if self.intentionSucceded:
        self.intentionSucceded = False
        self.D = self.options_second()
        self.I = self.filter_second()
        self.currentPlan = self.plan_second().pop()


    def execute(self):
        """Executes the plan, action by action."""

        if len(self.currentPlan) > 0:
            currentAction = self.currentPlan.pop(0)
            new_position = (self.model.Store.positions[self][0] +  currentAction[0],
                            self.model.Store.positions[self][1] + currentAction[1])

            if new_position not in [self.model.Store.positions[robot] for robot in self.model.robots if robot != self]:
              self.model.Store.move_by(self, currentAction)

            else:
              pass
              # print(f"Robot {self.id} encontró una colisión y no se movio")
            currentAction = (0, 0)

        else:
            self.intentionSucceded = True
            currentAction = (0, 0)

        if currentAction != (0, 0):
          self.model.Store.move_by(self, currentAction)


    def initBeliefs(self, initPos):
        """Initializes the Belief system by instantiating the first concepts from the ontology."""
        place = Place(at_position=str(initPos))
        self.this_robot = Robot(is_in_place=[place])

    def initIntentions(self):
        """Provides the first Intention, which in this case is empty."""
        self.intentionSucceded = True
        self.I = None

    def setup(self):
        """Initial setup for the agent."""
        self.agentType = 0
        self.firstStep = True
        self.currentPlan = []
        self.RobotStorage = 0
        self.RobotProcedure = 1


    def step(self):
        """Performs a step in the simulation."""
        if self.firstStep:
            initPos = self.model.Store.positions[self]
            self.initBeliefs(initPos)
            self.initIntentions()
            self.firstStep = False

        if self.RobotStorage>0:
          self.firstStep = True
          self.BDI(self.see_second(self.model.Store))
        else:
          self.BDI(self.see(self.model.Store))

        self.execute()

    def update(self):
        pass

    def end(self):
        pass

class BoxAgent(ap.Agent):

    #Setup
    def setup(self):
        self.agentType = 1

    #Step
    def step(self):
        pass

    #Update
    def update(self):
        pass

    #End
    def end(self):
        pass

class GoalAgent(ap.Agent):

  def setup(self):

    self.agentType = 2
    self.pile = 0

  def step(self):
    pass

  def update(self):
    pass

  def end(self):
    pass

class StoreModel(ap.Model):

  def setup(self):

    self.robots = ap.AgentList(self,self.p.robots,RobotAgent)
    self.boxes = ap.AgentList(self,self.p.box,BoxAgent)
    self.goals = ap.AgentList(self,self.p.goals,GoalAgent)

    self.Store = ap.Grid(self,self.p.storeSize,track_empty=True)

    self.Store.add_agents(self.robots,random = True,empty = True)
    self.Store.add_agents(self.boxes,random = True,empty = True)
    self.Store.add_agents(self.goals,random = True,empty = True)


  def get_pile(self):
    return self.goals.pile

  def step(self):
    self.robots.step()
    self.boxes.step()
    self.goals.step()
    step_positions = dict()
    [step_positions.setdefault(str(robot).replace("Agent (Obj ", '').replace(")", ''), self.model.Store.positions[robot]) for robot in self.model.robots if robot != self]
    [step_positions.setdefault(str(box).replace("Agent (Obj ", '').replace(")", ''), self.model.Store.positions[box]) for box in self.model.boxes if box != self]
    [step_positions.setdefault(str(goal).replace("Agent (Obj ", '').replace(")", ''), self.model.Store.positions[goal]) for goal in self.model.goals if goal != self]
    positions.append(step_positions)
    # print(positions)

    for robot in self.robots:
      if robot.RobotStorage<1:
        for box in self.boxes:
          if box in self.Store.positions and self.Store.positions[box] == self.Store.positions[robot]:
            robot.RobotStorage += 1
            self.Store.remove_agents(box)
            self.boxes.remove(box)
            # print(f"Robot {robot.id} ha recogido una caja. Total: {robot.RobotStorage}")
            break
      else:
        for goal in self.goals:
          if goal in self.Store.positions and self.Store.positions[goal] == self.Store.positions[robot]:
            if goal.pile < 5:
              goal.pile += 1
              robot.RobotStorage -= 1
              # print(f"Robot {robot.id} ha subido una caja. Total: {robot.RobotStorage}")
              # print(f"Goal {goal.id} ha recibido una caja. Pila: {goal.pile}")
            else:
              # print(f"Goal {goal.id} ha llegado a su límite de cajas.")
              self.goals.remove(goal)
              self.Store.remove_agents(goal)
              break




    if len(self.boxes) == 0 and all(robot.RobotStorage == 0 for robot in self.robots):
      print("Fin de la simulación")
      self.stop()

    #if len(self.boxes == self.robots.RobotStorage):
      #self.stop()
    # for robot in self.robots:
       # print(f"Robot {robot.id} tiene {robot.RobotStorage} cajas.")

  def update(self):
    pass

  def end(self):
    pass

#A FUNCTION TO ANIMATE THEE SIMULATION

def animation_plot(model, ax):
    agent_type_grid = model.Store.attr_grid('agentType')
    ap.gridplot(agent_type_grid, cmap='Accent', ax=ax)
    ax.set_title(f"Robot en almacen \n Time-step: {model.t}, ")

#SIMULATION PARAMETERS

#a random variables (0,1)
r = random.random()

#parameters dict
parameters = {
    "robots" : 5,     #Amount of Hunters
    "box" : 15,      #Amount of coins
    "goals" : 5,      #Amount of goals
"storeSize" : (15,15),      #Grid size
    "steps" : 100,          #Max steps
    "seed" : 13*r           #seed for random variables (that is random by itself)
}

#============================================================================0

#SIMULATION:

#Create figure (from matplotlib)
# fig, ax = plt.subplots()

#Create model
def start():
  model = StoreModel(parameters)


  #Run with animation
  #If you want to run it without animation then use instead:
  model.run()
  # animation = ap.animate(model, fig, ax, animation_plot)
  #This step may take a while before you can see anything

  #Print the final animation
  # IPython.display.HTML(animation.to_jshtml())