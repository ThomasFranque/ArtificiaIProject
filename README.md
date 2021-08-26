# AI Project

By Tomás Franco, a21803301

## Overview

![Overview](https://github.com/ThomasFranque/Modularia/blob/master/imgs/Overview.gif)

This project consists of a crowd simulation at an event. With explosions.

Table of contents:

- Finite State Machines
- Pathfinding (with Unity's [Navmesh](https://docs.unity3d.com/ScriptReference/AI.NavMesh.html))
- Static agent heuristic (static Brain)
- Agent Panic and it's propagation
- Fire propagation

## FSM

Each agent is controlled by a Finite State Machine that "Loops around".
For this technique, unity's animator system was used to achieve the desired result. More on that ahead.
After each "Loop" it asks the brain what it should do next. 

### The Approach

As mentioned before, Unity's animators were used to create the agent's FSM.

### Logic

![BaseTreeUML](https://github.com/ThomasFranque/Modularia/blob/master/imgs/BehaviourTreeUML.png)

- - - - Meter Screenshot

Each agent has 3 High-level states:

- Stay/Idle
- Moving

Since, in this simulation, all the agents do is move around and Using unity's [animator behaviour scrips](https://docs.unity3d.com/ScriptReference/StateMachineBehaviour.html), im able to have this high-level abstraction that allows me to be more flexible when implementing more behaviours to existing systems.

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

// I LEFT HERE, CONTINUE FROM HERE

#### Sequences

The sequences linearly call children after the completion of the previous.

An action based system is used to determine when they should call the
next children in line. The `OnComplete` action is called upon the completion of a
component, that means, if a behaviour takes some time to finish, only when the
`OnComplete` is called will it move to the next.

The `OnComplete` is communicated to every called component and reset on the
`Execute()` method. The only exception to this is on the sequences themselves,
they do not pass their `OnComplete` to their called children, they instead give
their own and keep the previous to themselves, allowing them to keep moving.
When there are no more children to execute, they call the previous `OnComplete`,
allowing for previous sequences to keep moving.

#### Leafs

The leafs approach was a little different than the usual.

Unity has `MonoBehaviours` that can be attached to _Game Objects_ and have all
their info. And the _abstract_ leaf class `ModularBehaviour` extends from it. This
allows for behaviours to be independent from the tree itself and use all of
unity's features
([`FixedUpdate`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html),
[`LateUpdate`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html),
[`Update`](https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html),
[`Transform`](https://docs.unity3d.com/ScriptReference/Transform.html)
and others)

And when they finish executing, they call the `Complete()` method provided by
the parent class.

Example:
Death Ray behaviour is called, the behaviour's OnExecute is called, allowing it
to startup the ray and shoot, and when that is done, it calls the `Complete()`
method that ends the leaf, disabling it if necessary to not affect performance.

The leafs also need some general behaviours/info, like distances, follow or
looking at. That is achieved by using separate components on the object that are
referenced in the parent class `ModularBehaviour` and all subclasses are free
to use them. They are all reset to defaults when a behaviour ends to prevent
previous behaviours states to traverse.

These general components are always required when a `ModularBehaviour` is added.
To ensure that happens the
[`[RequireComponent()]`](https://docs.unity3d.com/ScriptReference/RequireComponent.html)
attribute is used, guaranteeing that, every time a `ModularBehaviour` is added
that all the other general behaviours also get added.

These are: `Follow` `ProximityChecker` and `SmoothLookAt`.

### Enemy Tree Generation

![TreeUML](https://github.com/ThomasFranque/Modularia/blob/master/imgs/BehaviourTreeWGenUML.png)

#### Enemy generation

There are 3 types: Shooter, Brawler and Tank.

Each enemy has a core that defines its base type and preset behaviours of that
type.

Then, 0 to 3 limbs are attached based on the game difficulty,
that define additional behaviours as well as stat changes
(but that doesn't matter here).

#### Unity Flexibility

By using custom inspectors, I was able to handpick which behaviours a part had
for the generator to use.

##### Composed behaviours

Are sort of pre-made tree components

Are a collection of "Raw Behaviours" (behaviours that are not composed) and
composed behaviours. Reflection is used to get the raw behaviours.

![ComposedInspector](https://github.com/ThomasFranque/Modularia/blob/master/imgs/ComposedExample.png)

##### Modulariu Part Profile

(Enemies are called Modularius)

Are what behaviours can that part have. And other info.

![PartInspector](https://github.com/ThomasFranque/Modularia/blob/master/imgs/PartExample.png)

#### Generated Tree

The tree works as follows:

>     [X] - Random Selector
>     [>] - Sequential Selector
>
>                          [>]Main Selector          --> Layer 0
>                           /            \
>                     [X]Attack          Idle        --> Layer 1
>                       Selector         Leaf
>                  /      |     \
>        [X]Shooter  [X]Brawler  [X]Tank             --> Layer 2
>           Selector    Selector    Selector
>              |          |         |
>             ...        ...       ...               --> Layer 3...

All the behaviours and composed behaviours will be added to it's
respective selector.

If a core has all limbs of its type, it will be considered an elite
and add the respective type special composed behaviour.

The Layer 2 Selector weights will be determined by taking into
account the core type and limbs, the core having an weight of 0.8
while limbs have 0.2 (Do not mix weights with influences. Influences
were supposed to be the stats influence on the core, having nothing
to do with the tree).

### Behaviour Tree References

[Behaviour trees for AI - Chris Simpson](https://www.gamasutra.com/blogs/ChrisSimpson/20140717/221339/Behavior_trees_for_AI_How_they_work.php)

### What could have been done better

A parent class for the three `ITreeComponents`

A better `OnComplete` event handling

## Procedural Level Generation

![GenUML](https://github.com/ThomasFranque/Modularia/blob/master/imgs/LevelGenerationUML.png)

The level generation is pretty straightforward.
A main branch is generated and when that is over, sub-branches are generated
from it. Lastly, room doors on branch intersections and same-branch door intersections open up.

On the last room of the main branch, an exit is placed.

The rooms are pre-made with a size of 30x30 units.

Seeded generation is supported for same results every time.

![Seed-7262020](https://github.com/ThomasFranque/Modularia/blob/master/imgs/Gen7262020.png "Generation With seed 7262020")

![Seed-1234567](https://github.com/ThomasFranque/Modularia/blob/master/imgs/Gen1234567.png "Generation With seed 1234567")

### Parameters

There is a direction change parameter that determines whether the generator
should go a different direction or not after a new room is created.

There is a sub-branch chance. After the main branch is generated, for every room
in it, it will roll for a sub-branch using that chance.

### Generation References

[Spelunky Generation - GMTK](https://www.youtube.com/watch?v=Uqk5Zf0tw3o)

## A* Pathfinding

![PathfindUML](https://github.com/ThomasFranque/Modularia/blob/master/imgs/PathfindingUML.png)

Due to lack of time, this approach was the least homebrewed of the bunch, an
implementation reference was taken from Sebastian Lague on Youtube on his A*
Pathfinding series. Pre-made code was not taken from his given resource.
Everything was written from scratch.

Even though implementation was simplified I still needed to make it work with
the procedural levels and make it adapt to dynamic surroundings.

![PathfindExample](https://github.com/ThomasFranque/Modularia/blob/master/imgs/Pathfind.png)

### Dynamic Surroundings

For this, every time the algorithm tries to move to a certain tile, it performs a
[`Physics.CheckBox()`](https://docs.unity3d.com/ScriptReference/Physics.CheckBox.html)
provided from Unity that tells if anything is overlapping with that tile position.

This allows for environment physics to take place and affect the AI.

### Procedural Levels Link

The initial idea was to have a big grid for the entire map, but time was running
out and had to cut that idea.
Instead, I made it so that every room is individual and doors close upon player
entry, spawns some enemies and, when they are all dead, doors open up again.

### A* Reference

[A* Implementation - Sebastian Lague](https://www.youtube.com/watch?v=mZfyt03LDH4)

[A Simple A* Path-Finding Example in C# - TwoCats Blog](https://web.archive.org/web/20170505034417/http://blog.two-cats.com/2014/06/a-star-example/)

## Known Issues (so far)

### Behaviour Tree

- On the enemy generated tree entering the idle behaviour will cause the tree
to stop and not keep running (not sure if it is a behaviour problem or a tree problem).
- The heal behaviour will sometimes trigger many times in a row (more often
  on brawlers with tank limbs)

### Enemy Generation

- The only known issue is a huge game design flaw on chances that prevents
the generation from being interesting.

### Level Generation

- No known issues.

### Pathfinding

- Enemies will get stuck on obstructed tiles if they happen to step on them
- The game performance is awfully hit if the player stands in an obstructed
tile due to long pathfinding searches (implementing an obstacle avoidance
algorithm should fix that).
- The enemy movement is not smooth and the path can be clearly seen

### Code

- The huge lack of and inconsistency on the documentation

## Used Assets

- [Volumetric 3D Lasers](https://assetstore.unity.com/packages/vfx/particles/spells/volumetric-3d-lasers-104580)

- [Epic Toon FX](https://assetstore.unity.com/packages/vfx/particles/epic-toon-fx-57772)

- [RPG Monster Duo](https://assetstore.unity.com/packages/3d/characters/creatures/rpg-monster-duo-pbr-polyart-157762)

- [Lovely animals pack](https://assetstore.unity.com/packages/3d/characters/animals/lovely-animals-pack-92629#content)

## General References

[Fisher–Yates shuffle solution](https://stackoverflow.com/questions/273313/randomize-a-listt)

[Wait one frame solution](https://forum.unity.com/threads/how-to-wait-for-a-frame-in-c.24616/)

The pathfinding algorithm was discussed with André Vitorino a21902663.

General feedback and suggestions from Rodrigo Pinheiro a21802488.

## Dev Keys

> R - Generate new level.
>
> G - View generated map.
>
> B - Check enemy behaviours overlay.
>
> P - To pause.

(I - To access enhancements)



https://forum.unity.com/threads/navmeshagent-and-triggers-help-plz.127371/
https://forum.unity.com/threads/how-to-define-movement-areas-for-each-character.447619/
https://docs.unity3d.com/Manual/nav-AreasAndCosts.html
https://answers.unity.com/questions/366157/mouse-click-to-world-space.html
https://www.reddit.com/r/Unity3D/comments/9359us/how_would_you_make_navmesh_agents_avoid_each_other/
https://docs.unity3d.com/Manual/nav-CreateNavMeshAgent.html
https://stackoverflow.com/questions/141088/what-is-the-best-way-to-iterate-over-a-dictionary