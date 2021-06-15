# Senior Project

### Contents
1. [Introduction](https://github.com/TheJimmyGod/Senior_Project#introduction)
2. [How to use](https://github.com/TheJimmyGod/Senior_Project#how-to-use)
3. [Features](https://github.com/TheJimmyGod/Senior_Project#features)
4. [Principles of Multi-threading work](https://github.com/TheJimmyGod/Senior_Project#principles-of-multi-threading-work)
5. [Examples](https://github.com/TheJimmyGod/Senior_Project#examples)
6. [Acknowledgement](https://github.com/TheJimmyGod/Senior_Project#acknowledgement)

---

### Introduction
**Stealthos** is a senior project which utilizes multi-threading for AI pathfinding. The project was motivated by the *Commandos series*, and started from the curiosity of **how multithreaded pathfinding affects the performance**. For implementation, I programmed a ```C++ queuing system for executing DFS and  A* search using threads and tasks```, ```state machine for controlling the agents```, and ```visual sensor for perception```.

---

### How to use
#### *Requirement*
###### Unity 2019.4.18f1

#### 1. Set Grid size from plane object
![Screen](https://github.com/TheJimmyGod/Senior_Project/blob/main/Multithreading_With%20AI/Documents/Menu0.PNG)

#### 2. Set entity count and thread count from AI object
![Screen](https://github.com/TheJimmyGod/Senior_Project/blob/main/Multithreading_With%20AI/Documents/Menu1.PNG)

#### 3. Start to play the scene to watch or control

---

### Features
- Implemented ```AI World``` and ```Girds``` allow to agents have AI cores in game world
- Implemented ```Queuing System``` using DFS and A* Search executing multi-threads and multi-tasks
- Implemented ```Finite State Machine``` supports to agents have behaviors included: ```Idle```, ```Find```, and ```Walk```
- Implemented ```Visual Sensor``` supports to agents seek the player out in their sight
- Implemented ```UI System``` supports to list of agent's information included:
  - **Entire**
    - ```Pathfind Search```: Displaying what pathfinding search it uses
    - ```Approximate Time```: Calculating total average time of how much executing time they work. If thread usage is more than 1, it will compare executing times for finding out the shortest time, then computing on Approximate time to be average
    - ```Round```: Round pool that displays the total output of the agents (limited 4) 
    - ```Thread Vaild Values```: Displaying how much threads you would use in game play
  - **Individual**
    - ```State image```: Displaying present state of the agent
    - ```Average time```: Calculating each average time of how much executing time they work
    - ```Type```: Property of what thread type is
    - ```Count```: Listing how much time the agent execute pathfinding search

---

### Principles of Multi-threading work
The main thread sends a new request to the thread manager, and is assigned to one or many threads. In one or many threads, they execute ```DFS``` or ```A* Search``` in loop until finding the best outcome, and then the outcome will return to the main thread in order to call back a function where the path is.

---

### Examples
![Screen](https://github.com/TheJimmyGod/Senior_Project/blob/main/Multithreading_With%20AI/Documents/One.gif)

The screenshot is to showcase of visual sensor when sight in one of entities has reached nearby the player.

![Screen](https://github.com/TheJimmyGod/Senior_Project/blob/main/Multithreading_With%20AI/Documents/Two.gif)

The screenshot is to showcase of printing out the outcome for all agents when the round has been finished.

![Screen](https://github.com/TheJimmyGod/Senior_Project/blob/main/Multithreading_With%20AI/Documents/100Entities.gif)

The screenshot displays the outcome when the entities are 100 and threads are 4 counts. The game maintains working pathfinding search, even though the performance is getting slower.

---

### Acknowledgement
[Richard Zampieri](https://github.com/rsaz) the instructor
