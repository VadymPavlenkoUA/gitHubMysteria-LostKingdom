# Mysteria: Lost Kingdom
### Unity, C#, Python, Flask, Ollama, JSON, REST API

An intelligent educational RPG game with an adaptive learning system, AI assistant, and gamification elements.


## About the project

The main goal of the project was to demonstrate the concept of an interactive learning system that combines role-playing game mechanics, adaptive learning, and artificial intelligence technologies. Unlike traditional educational platforms, the user interacts with an open game world, completes quests, solves educational tasks (Math, English, and Programming), and gradually improves their character's stats. Task difficulty automatically adapts based on the player's performance.

The project follows a client-server architecture: the client side is built with Unity (C#), and the server side with Python Flask. Interaction between components is handled via a REST API using JSON, and AI functionality is implemented through integration of a local LLM via Ollama.


## Key Features

* open RPG world
* adaptive learning system
* AI assistant
* quest system
* NPCs
* dialogues
* inventory
* crafting
* trading
* character progression
* combat system
* day/night cycle
* progress saving
* AI hint generation
* learning subjects (Math, English, Programming)


## Adaptive Learning System

Task difficulty adaptation is based on an assessment of the user's knowledge level, the forgetting effect, and behavioral indicators.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/2daab5bd-197f-4220-b5cb-48967dbf463f" />

### Bayesian Knowledge Tracing

The probability that a skill has been mastered after a correct answer:

<img width="480" height="68" alt="5" src="https://github.com/user-attachments/assets/e9fa52df-f7c0-4d6b-bf52-57aec12ca99f" />



Similarly, in the case of an incorrect answer, the probability is updated using the following formula:

<img width="497" height="68" alt="image" src="https://github.com/user-attachments/assets/eb017c78-79ef-4d60-b2d7-15763f1ff5cb" />

where **S** is the probability of a slip (mistake), **G** is the probability of a random guess, **T** is the probability that the user learned the new material during the task, and **t** is the time point corresponding to a specific learning interaction.

## Ebbinghaus Forgetting Formula
The level of retained knowledge decreases exponentially over time:
**𝑅(𝑡)=𝑒^(−𝑡/𝜆)**
where **R(t)** is the knowledge retention coefficient, **t** is the time elapsed since the last review of the material, and **λ** is the memory stability parameter, reflecting the individual characteristics of the learner.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/c9262328-9765-4f6e-bc3a-99be3071ddf1" />



## AI Assistant

The project includes an integrated AI assistant that operates via a local Flask server and LLM.

<img width="500" height="250" alt="5" src="https://github.com/user-attachments/assets/44d23a5a-b679-4c3b-bb87-e01de24b047b" />

Key Features:
* Explains learning material within the game context
* Answers user questions
* Adapts explanation complexity to the user's knowledge level
* Provides hints and motivational support
* Uses an emotional model (trust, mood, mystery)
* Supports short-term and long-term dialogue memory

<img width="500" height="300" alt="5" src="https://github.com/user-attachments/assets/3889e84b-76e3-4328-856e-f3b1a7f46fc3" />

### AI Assistant Technical Implementation

The client side (Unity) builds the request context based on the user's in-game data (statistics, quests, learning progress), the Python server handles request processing and session management, and the local language model (Ollama) generates a response based on the provided context; the resulting output is returned to the client, where it is used to update the game state and user interface.

<img width="700" height="180" alt="5" src="https://github.com/user-attachments/assets/405c3fb2-0560-48e1-a3d1-d4394be7e75c" />


## Data Saving
Saving is implemented through the **ISaveable** interface, which allows the state of all game components to be centrally saved to JSON files via the **SaveManager**.

<p>
  <img src="https://github.com/user-attachments/assets/6637e32f-18af-44e1-a4a3-c9f64778152d" height="300" />
  <img src="https://github.com/user-attachments/assets/936cfa8f-bb52-473b-9679-bf6f706fc617" height="300" />
</p>


## Game Mechanics

### Inventory
A system for storing items with properties that affect character stats and gameplay.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/1dd66105-fdc5-4bf0-ac34-d7344ca94a41" />

### Crafting
Creating items by combining resources according to specified recipes.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/5181aca3-8aaa-480e-a279-fa1de56624be" />

### Skills
A mechanic for developing character stats that determines interaction effectiveness, gameplay, and task difficulty.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/528418d3-d1b6-4802-af97-09a5f4ff609d" />

### Dialogues/NPCs
An interactive system for engaging with characters and the AI assistant, featuring context-aware responses.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/54f83c72-cfa5-419f-ae39-e12b51a7598d" />

### Trading
A mechanic for exchanging resources between the player and NPCs to advance equipment and progress.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/c6eadec3-79e4-4591-b642-71b2a8b6fe9c" />

### Quests
A task system with progression that guides the user's learning and gameplay process.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/a029b770-3661-4b80-b071-169f060c8698" />

### Combat Mechanics
Combat mechanics implement the character's interaction with enemies and include attack mechanics, taking damage, resource consumption, and evasion movements (dodge rolls).

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/828dd082-8d6c-4d6a-927e-ffc6c1b089c4" />


