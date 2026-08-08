# Mysteria: Lost Kingdom
### Unity, C#, Python, Flask, Ollama, JSON, REST API

An intelligent educational RPG game with an adaptive learning system, AI assistant, and gamification elements.


## About the project

The main goal of the project was to demonstrate the concept of an interactive learning system that combines role-playing game mechanics, adaptive learning, and artificial intelligence technologies. Unlike traditional educational platforms, the user interacts with an open game world, completes quests, solves educational tasks (Math, English, and Programming), and gradually improves their character's stats. Task difficulty automatically adapts based on the player's performance.

The project follows a client-server architecture: the client side is built with Unity (C#), and the server side with Python Flask. Interaction between components is handled via a REST API using JSON, and AI functionality is implemented through integration of a local LLM via Ollama.


## Key Features

✔ open RPG world
✔ adaptive learning system
✔ AI assistant
✔ quest system
✔ NPCs
✔ dialogues
✔ inventory
✔ crafting
✔ trading
✔ character progression
✔ combat system
✔ day/night cycle
✔ progress saving
✔ AI hint generation
✔ learning subjects:
    • Math
    • English
    • Programming


## Adaptive Learning System

Task difficulty adaptation is based on an assessment of the user's knowledge level, the forgetting effect, and behavioral indicators.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/2daab5bd-197f-4220-b5cb-48967dbf463f" />

### Байєсівське простежування знань

Ймовірність того, що навичка засвоєна після правильної відповіді:

<img width="480" height="68" alt="5" src="https://github.com/user-attachments/assets/e9fa52df-f7c0-4d6b-bf52-57aec12ca99f" />



Аналогічно, у разі неправильної відповіді, ймовірність оновлюється за формулою:

<img width="497" height="68" alt="image" src="https://github.com/user-attachments/assets/eb017c78-79ef-4d60-b2d7-15763f1ff5cb" />

де S – ймовірність помилки, G – ймовірність випадкового вгадування, T – ймовірність того, що користувач вивчив новий матеріал у процесі виконання завдання, а t – момент часу, що відповідає конкретній навчальній взаємодії.

## Формула забування Ебінгауза
Рівень засвоєних знань з часом експоненційно зменшується:
𝑅(𝑡)=𝑒^(−𝑡/𝜆)
де R(t) – коефіцієнт збереження знань, t – час, що минув з моменту останнього повторення матеріалу, а λ – параметр стабільності пам’яті, який відображає індивідуальні особливості учня.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/c9262328-9765-4f6e-bc3a-99be3071ddf1" />



## AI-помічник

Проєкт містить інтегрованого AI-помічника, який працює через локальний сервер Flask та LLM.

<img width="500" height="250" alt="5" src="https://github.com/user-attachments/assets/44d23a5a-b679-4c3b-bb87-e01de24b047b" />

Основні можливості:
* Пояснення навчального матеріалу в ігровому контексті
* Відповіді на запитання користувача 
* Адаптація складності пояснень відповідно до рівня знань 
* Надання підказок та мотиваційна підтримка 
* Використання емоційної моделі (trust, mood, mystery) 
* Підтримка коротко- та довготривалої пам’яті діалогу

<img width="500" height="300" alt="5" src="https://github.com/user-attachments/assets/3889e84b-76e3-4328-856e-f3b1a7f46fc3" />

### Технічна реалізація АІ-помічника

Клієнтська частина (Unity) формує контекст запиту на основі ігрових даних користувача (статистика, квести, навчальний прогрес), Python-сервер виконує обробку запиту та керування сесією, а локальна мовна модель (Ollama) генерує відповідь з урахуванням переданого контексту; отриманий результат повертається до клієнта, де використовується для оновлення ігрового стану та інтерфейсу користувача.

<img width="700" height="180" alt="5" src="https://github.com/user-attachments/assets/405c3fb2-0560-48e1-a3d1-d4394be7e75c" />


## Збереження даних
Збереження реалізовано через інтерфейс **ISaveable**, який дозволяє централізовано зберігати стан усіх ігрових компонентів у JSON-файли через **SaveManager**.

<p>
  <img src="https://github.com/user-attachments/assets/6637e32f-18af-44e1-a4a3-c9f64778152d" height="300" />
  <img src="https://github.com/user-attachments/assets/936cfa8f-bb52-473b-9679-bf6f706fc617" height="300" />
</p>


## Ігрові механіки

### Інвентар
Система зберігання предметів з їх властивостями, що впливають на характеристики персонажа та геймплей.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/1dd66105-fdc5-4bf0-ac34-d7344ca94a41" />

### Майстрування
Створення предметів шляхом поєднання ресурсів за заданими рецептами.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/5181aca3-8aaa-480e-a279-fa1de56624be" />

### Навички
Механіка розвитку характеристик персонажа, що визначає ефективність взаємодії, геймплей та складність завдань.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/528418d3-d1b6-4802-af97-09a5f4ff609d" />

### Діалоги/NPC
Інтерактивна система взаємодії з персонажами та ШІ-асистентом із контекстними відповідями.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/54f83c72-cfa5-419f-ae39-e12b51a7598d" />

### Торгівля
Механіка обміну ресурсів між гравцем і NPC для розвитку спорядження та прогресу.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/c6eadec3-79e4-4591-b642-71b2a8b6fe9c" />

### Квести
Система завдань із прогресією, що спрямовує навчальний та ігровий процес користувача.

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/a029b770-3661-4b80-b071-169f060c8698" />

### Бойова механіка
Бойова механіка реалізує взаємодію персонажа з противниками та включає механіки атаки, отримання шкоди, витрат ресурсів, а також рухи ухилення (перекати).

<img width="525" height="300" alt="5" src="https://github.com/user-attachments/assets/828dd082-8d6c-4d6a-927e-ffc6c1b089c4" />


