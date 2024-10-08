# TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
# Python server to interact with Unity via POST
# Sergio Ruiz-Loza, Ph.D. March 2021
#Actualizado por Axel Dounce, PhD

from http.server import BaseHTTPRequestHandler, HTTPServer
import logging
import json
import threading 
import agentpy as ap
import pathfinding as pf
import matplotlib.pyplot as plt
from owlready2 import *
import itertools
import random
import IPython
import math

class Server(BaseHTTPRequestHandler):
    
    def _set_response(self):
        self.send_response(200)
        self.send_header('Content-type', 'text/html')
        self.end_headers()
        
    def do_GET(self):
        logging.info("GET request,\nPath: %s\nHeaders:\n%s\n", str(self.path), str(self.headers))
        response_data = get_response()
        self._set_response()

        json_print = json.dumps(json.loads(response_data), indent = 2)

        html_response = f"""
        <html>
        <head>
            <title>JSON</tile>
        </head>
        <body>
            <pre>{json_print}</pre>
        </body>
        </html>
        """

        self.wfile(html_response.enconde("utf-8"))


    def do_POST(self):
        content_length = int(self.headers['Content-Length'])
        #post_data = self.rfile.read(content_length)
        post_data = json.loads(self.rfile.read(content_length))
        #logging.info("POST request,\nPath: %s\nHeaders:\n%s\n\nBody:\n%s\n",
                     #str(self.path), str(self.headers), post_data.decode('utf-8'))
        logging.info("POST request,\nPath: %s\nHeaders:\n%s\n\nBody:\n%s\n",
                     str(self.path), str(self.headers), json.dumps(post_data))
      
      
        # Aquí se procesa lo el cliente ha enviado, y se construye una respuesta.
        response_data = post_response(post_data)
        
        
        self._set_response()
        #self.wfile.write("POST request for {}".format(self.path).encode('utf-8'))
        self.wfile.write(str(response_data).encode('utf-8'))


def run(server_class=HTTPServer, handler_class=Server, port=8585):
    logging.basicConfig(level=logging.INFO)
    server_address = ('', port)
    httpd = server_class(server_address, handler_class)
    logging.info("Starting httpd...\n") # HTTPD is HTTP Daemon!
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:   # CTRL+C stops the server
        pass
    httpd.server_close()
    logging.info("Stopping httpd...\n")
    
    #======================Procesamiento de datos de cliente=========================
    
def post_response(data):
    """
    Función para procesar los datos que vienen del cliente (mediante POST) en forma de JSON.
    Se construye un JSON para la respuesta al cliente.
    Se retorna la respuesta.
    
    Ejemplo:
    
        x = data['x'] * 2
        y = data['y'] * 2
        z = data['z'] * 2
        
        new_position = {
            "x" : x+1,
            "y" : y-1,
            "z" : z
        }
        
        return new_position
    """
    
    return None
    
def get_response(data):
    """
    Función construir un JSON para la respuesta al cliente (GET).
    Se retorna la respuesta.
    
    Ejemplo:
    
        act = wealth_transfer
        
        
        return act
    """
    
    return None
    
    
#===================Definición de Agentes y simulación (Model)=================
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

onto.save(file="onto.owl", format = "rdfxml")

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
      print(thePlantoGoal)

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
              print(f"Robot {self.id} encontró una colisión y no se movio")
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

    grid_width = self.p.storeSize[0]
    grid_height = self.p.storeSize[1]


    self.Store.add_agents(self.robots,random = True,empty = True)
    self.Store.add_agents(self.boxes,random = True,empty = True)

    specific_positions = [
        (0,0),
        (0,grid_height-1),
        (grid_width-1,0),
        (grid_width-1,grid_height-1),
        (grid_width//2, 0),
        (grid_width//2,grid_height-1),
        (0,grid_height//2),
        (grid_width-1,grid_height//2)
    ]



    while(len(self.goals) > len(specific_positions)):
      #specific_positions.append((random.randint(0,grid_width-1),random.randint(0,grid_height-1)))
      self.goals.remove(random.choice(self.goals))


    for goal, pos in zip(self.goals,specific_positions):
      self.Store.add_agents([goal],[pos], empty=True)



  def get_pile(self):
    return self.goals.pile

  def step(self):
    self.robots.step()
    self.boxes.step()
    self.goals.step()

    for robot in self.robots:
      if robot.RobotStorage<1:
        for box in self.boxes:
          if box in self.Store.positions and self.Store.positions[box] == self.Store.positions[robot]:
            robot.RobotStorage += 1
            self.Store.remove_agents(box)
            self.boxes.remove(box)
            print(f"Robot {robot.id} ha recogido una caja. Total: {robot.RobotStorage}")
            break
      else:
        for goal in self.goals:
          if goal in self.Store.positions and self.Store.positions[goal] == self.Store.positions[robot]:
            if goal.pile < 5:
              goal.pile += 1
              robot.RobotStorage -= 1
              print(f"Robot {robot.id} ha subido una caja. Total: {robot.RobotStorage}")
              print(f"Goal {goal.id} ha recibido una caja. Pila: {goal.pile}")
            else:
              print(f"Goal {goal.id} ha llegado a su límite de cajas.")
              self.goals.remove(goal)
              self.Store.remove_agents(goal)
              break




    if len(self.boxes) == 0 and all(robot.RobotStorage == 0 for robot in self.robots):
      print("Fin de la simulación")
      self.stop()

    for robot in self.robots:
       print(f"Robot {robot.id} tiene {robot.RobotStorage} cajas.")

  def update(self):
    pass

  def end(self):
    pass




#==================================Main===========================

if __name__ == '__main__':
    from sys import argv
    
    #Iniciar hilo del servidor    
    p = threading.Thread(target=run, args = tuple(),daemon=True)
    p.start()
    
    #Correr simulación (de preferencia no especifiquen los steps)
    #parameters={}
    #model = AgentModel(parameters)
    #results = model.run()



