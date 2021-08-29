# AI Project

By Tomás Franco, a21803301

## Overview

![Overview](https://github.com/ThomasFranque/ArtificiaIProject/blob/master/Images/Overview.gif)

This project consists of a crowd simulation at an event. With explosions.

Table of contents:

- Finite State Machine and states
- Pathfinding (with Unity's [Navmesh](https://docs.unity3d.com/ScriptReference/AI.NavMesh.html))
- Static Agent Brain
- Explosions / Agent Panic and it's propagation
- Fire propagation

## FSM and States

Each agent is controlled by a Finite State Machine that "Loops around".
For this technique, unity's animator system was used to achieve the desired result. More on that ahead.
After each "Loop" it asks the brain what it should do next.

### FSM Approach

As mentioned before, Unity's animators were used to create the agent's FSM.
The animator acts as a High-level AI component, only doing what the agent needs to. Each agent is controlled by a static brain that defines what should be done next.

![Tree parameters](https://github.com/ThomasFranque/ArtificiaIProject/blob/master/Images/AgentStateMachine.png)

The individual agent will handle all necessary changes to its behaviour and notify the State Machine if and when it happens.

### Logic

![Tree](https://github.com/ThomasFranque/ArtificiaIProject/blob/master/Images/AnimatorParameters.png.PNG)


Each agent has 3 High-level states:

- Stay/Idle
- Moving
- Panic

Since, in this simulation, all the agents do is move around and, using unity's [animator behaviour scrips](https://docs.unity3d.com/ScriptReference/StateMachineBehaviour.html), im able to have this high-level abstraction that allows me to be more flexible when implementing more behaviours to existing systems.

#### States

Controlling the Animator, we have a few more states to help differentiate what the agents are doing:

    none,
    watch_concert_positive,
    watch_concert_negative,
    move_to_concert,
    eat,
    move_to_food,
    sit,
    move_to_open_space,
    panicking,
    explosion_victim

Each of those states can be broken down into the two High-Level states.

##### Stay/Idle States

Stay/Idle states are the states in which the AI is interacting with an area or in idle.

###### `watch_concert_positive` State

This state means the agent just arrived at a concert and is happily enjoying it.

    -- Bored
    +  Tired
    +  Hunger

###### `watch_concert_negative` State

This state means the agent is getting bored and tired of the current show, when it reaches a certain boredom threshold it will change to some other concert.

    +  Bored
    ++ Tired
    +  Hunger

###### `eat` State

This state means the agent is eating. It will stop eating when no longer hungry.

    ++ Bored
    -  Tired
    -- Hunger

###### `sit` State

This state means the agent is resting. When resting, if the agent is both tired and hungry, it will make its way to an eating area, since it is also possible to rest while eating.
It will stop eating when full.

    ++ Bored
    -- Tired
    +  Hunger

##### Moving States

Moving states are the states in which the AI is on the move, from one place to another.

###### `move_to_concert` State

This state means the agent is moving to a concert.
If coming from a concert, the AI will chose another concert that is not the one it is in.

_Note: As opposed to what was supposed to happen, the agents do **not** try to move as close as possible to the concert stage._

    +  Bored
    ++ Tired
    +  Hunger

###### `move_to_food` State

This state means the agent is moving to an eating area.
When moving to an eating area, the AI will choose an empty chair to sit on and eat as its destination.

    +  Bored
    ++ Tired
    +  Hunger

###### `move_to_open_space` State

This state means the agent is moving to an open area.
When moving to an open area, the AI will try to pick a space that has a minimum X distance to other agents if possible. Every iteration that happens reduces the minimum amount of space required by a little bit. When the area is too full, the AI will check which agents are further away from each other and sit in the middle of them.

    +  Bored
    ++ Tired
    +  Hunger

##### Panic States

Panic states are a mixture of both walk and idle states.
A panic state can occur at any given time, overriding any other action the agent could be doing.

###### `panicking` state

This state means the agent is panicking.
The agent will look for the closest exit and run at twice the walking speed. Warning others on the way.

    ++ Speed

###### `explosion_victim` state

This state means the agent was victim of an explosion and is crippled.
Upon entering this state there is a 50/50 chance it will start bleeding. The agent will also not be able to move for `1 + Random.value * 3` seconds. Once the paralyze timer is over, the bleeding over starts, the bleeding will occur for `5 + Paralyze_Time + Random.value * 5` seconds
If bleeding timer reaches 0, the agent dies.
The agent will look for the closest exit and run at `Base_Speed + Random.value * 0.4 + 0.1` speed. Warning others on the way.

    -- Speed
    +  50/50 Death Timer

## Pathfinding

### Pathfinding Approach

![Navmesh](https://github.com/ThomasFranque/ArtificiaIProject/blob/master/Images/NavemeshBake.PNG)

Unity's NavMesh system was used for Pathfinding. Unity also has a handy object avoidance behaviour for its Navmesh agents.

The agents have a cinematic movement type, since Navmesh agents do not support physics right out of the box.

Navmesh terrains have a value that indicates how hard it is to traverse. The higher the number, the harder it is to traverse. That way, I was able to make preferable paths for the agents to walk through.
When chaos takes place, these numbers will all be set to one, so the agents can take whatever course they see fit to get to the exit as fast as possible.

Navmesh Colliders are a great way to set up static obstacles with almost no effort. The obstacles will intersect the Navmesh and create holes that cannot be traversed.

### Static Brain

As previously mentioned, the agents refer to a Brain when the need to decide what to do next arises. This approach was chosen in hopes of aiding performance when dealing with many agent simulations.

The Brain also handles creation and destruction of agents.

The main feature of the brain is its Tick system. When an agent is created, it automatically subscribes to the `OnTick Action`.

#### Ticks

The ticks control the agent statistic changes (Hunger, Tiredness and Boredom).
When a tick occurs, the agents will process what to decrease or increase based in its current state. For example, if an agent is eating, it will decrease hunger and tiredness, but increase boredom.

For variety, upon creation, each agent is given a random value that increases or decreases the speed at which a stat changes, this makes it so an agent is more prone to get tired than others for example. This also applies for speed.

The tick speed can be adjusted in the inspector. With a default of a 5 seconds interval.

#### Heuristic

The brain answers to its agent's calling, telling them what to do next. It follows the following rules:

- Agent prioritizes resting, then food.
- If both tiredness and hunger are above 90%, moves to eating zone.
- While watching a concert, boredom will drop to 0% and then rise again to 50%.
- When watching a concert and boredom is above 50%, move to some other concert.
- If panicking, keep panicking.

### Explosions / Agent Panic

Explosions can be created on demand, when the user clicks anywhere on the map, an explosion will take place. Every agent that is within a certain radius will be affected in some way.

![Explosion](https://github.com/ThomasFranque/ArtificiaIProject/blob/master/Images/ExplosionAndFire.gif)

#### Explosions approach

The explosions are completely separate from the agent logic, at no given time are they expecting an explosion.

The explosions themselves are completely customizable in the explosioneer inspector.
When an explosion happens, the prefab of the explosion and its respective colliders will get scaled accordingly.

![Explosioneer](https://github.com/ThomasFranque/ArtificiaIProject/blob/master/Images/ExplosionRadiusExample.gif)

There are three different outcomes if an agent is caught in an explosion:

- Be caught on the outskirts, get scared and start panicking.
- Get caught near the heart of the explosion and get crippled.
- Be at the heart of the explosion and instantly die.

![Explosion Radius](Explosion.PNG.jpg)

#### Panic Approach

A Panic State can happen at any time to any agent from two possible sources, either be caught in one of the explosion radiuses or get warned by others panicking.

##### Panic propagation

The panic propagation approach is very flexible. It is an animator behaviour script that propagates any of the states.

![State propagation](https://github.com/ThomasFranque/ArtificiaIProject/blob/master/Images/StatePropagationInspector.PNG)

All it does is check if there is any agent within the given radius and propagates the given state, in this case, within 6.5 meters it propagates the state `Panicking`.

This also happens on the fire.

### Fire Propagation

A fire occurs when an explosion takes place, an explosion spawns a default of five fires.

#### Fire approach

The fire AI is probably the less smart, complex and performant of the bunch. All it does is have a cooldown. When the cooldown ends, it will chose some of its sides and propagate more fire to those chosen sides if they are not obstructed, either by aftermath of a previous fire, an existing fire or obstacle.

The fire lifetime consists of the deadly fire and the innocent aftermath, the aftermath creates an obstruction only for the fire that prevents it from moving there.

![Fire Propagation](https://github.com/ThomasFranque/ArtificiaIProject/blob/master/Images/FirePropagation.png)

## Stress test, possible optimizations and final thoughts

The simulation was able to just barely withstand 2000 agents at the same time.
I believe that most of the performance issues come from text and mesh renderers, although, the pathfinding component is most definitely what is taking more resources of all AI components.

A future iteration aimed at performance would be focused on the pathfinding and explosion behaviour, both from the fire and the agent parts.

This project, although not complete with missing features, was built from the ground up with great interest and dedication.

Some unity project walkthrough topics:

- Everything that can be customizable, like entrances/exits, explosions, etc... is in the `Managers Object` at the root of the scene hierarchy.
- The animator trees are beside the respective controller scripts.

Missing features:

- The concert area behaviour.
- Agents do not have a Flee movement type, only Seek.
- More complex panic behaviour (like sight).
- More interesting fire.
- Personal goal of a procedural level generation.

## Known Issues (so far)

### FSM, Brain and States issues

- The eating area, after looping through all empty spots, will get broken and agents will start to want to move to the same seats, causing them to be stuck.

### Pathfinding issues

- When in panic mode and fire spreading, the agents will stop moving to recalculate the path.
- Even if an agent is not in panic mode, if some other agent is, it will traverse the map as if it was running away, ignoring paths and going off-road.

### Fire propagation / Explosion issues

- Fire propagation might never stop.
- Poor optimization.

### Code issues

- Huge lack of documentation.

## Used Assets

Minecraft fire texture

## General References

[Navmesh and triggers](https://forum.unity.com/threads/navmeshagent-and-triggers-help-plz.127371/)

[Movement areas for each character](https://forum.unity.com/threads/how-to-define-movement-areas-for-each-character.447619/)

[Navmesh area costs](https://docs.unity3d.com/Manual/nav-AreasAndCosts.html)

[Mouse click to world space](https://answers.unity.com/questions/366157/mouse-click-to-world-space.html)

[Navmesh object avoidance](https://www.reddit.com/r/Unity3D/comments/9359us/how_would_you_make_navmesh_agents_avoid_each_other/)

[Creating a navmesh agent](https://docs.unity3d.com/Manual/nav-CreateNavMeshAgent.html)

[Best way to iterate through a dictionary](https://stackoverflow.com/questions/141088/what-is-the-best-way-to-iterate-over-a-dictionary)

## Outside help

Help structuring the state machine and its logic from Rodrigo Pinheiro.

Coding/logic references and Readme format help from my last year [AI Project Modularia](
https://github.com/ThomasFranque/Modularia)

## Dev Keys

> R - Reset.
>
> S - Speed up (hold)
