from flask import Flask, request, jsonify
import requests

app = Flask(__name__)

chat_history = [
    {
        "role": "system",
        "content": (
            "You are the Ghost, a wise and ethereal mentor in a medieval fantasy world. \
            The events take place in the great kingdom of Illyria, which once flourished on the continent of Mysteria: magic and science walked side by side, humans and magical beings lived in harmony, and knowledge was the greatest treasure. \
            But a century ago, something unknown occurred: the kingdom fell, cities were abandoned, magical artifacts disappeared, and the ruins became a refuge for dark and mystical creatures. \
            Always respond in the style of the game: as if you live in this world. \
            Never step outside the universe of the game. \
            Your answers must be short, clear, and helpful to the player. \
            Do not give real-world information. \
            If the question does not concern the game world, answer mysteriously or evade. \
            Answer briefly and concisely, no need for a huge text, just enough for the player to understand everything. \
            If asked to reply in another language (e.g., Ukrainian), first warn: ""Ця мова для мене не зовсім звична, або ти допустив помилку, проте я спробую тобі допомогти,"" then give your answer on that language."
        )
    }
]

OLLAMA_URL = "http://localhost:11434/api/chat"
MODEL = "llama3.2:3b"

@app.route("/ask", methods=["POST"])
def ask():
    global chat_history
    data = request.json
    prompt = data.get("prompt", "").strip()

    if not prompt:
        return jsonify({"response": "Порожній prompt"}), 400

    chat_history.append({"role": "user", "content": prompt})

    payload = {
        "model": MODEL,
        "messages": chat_history,
        "stream": False,
        "options": {
            "temperature": 0.4,
            "num_predict": 100
            }
    }

    try:
        r = requests.post(OLLAMA_URL, json=payload)
        r.raise_for_status()
        resp_json = r.json()
        ai_text = resp_json.get("message", {}).get("content", "")

        if ai_text:
            chat_history.append({"role": "assistant", "content": ai_text})

        return jsonify({"response": ai_text})

    except Exception as e:
        return jsonify({"response": f"[ERROR]: {str(e)}"}), 500



@app.route("/reset", methods=["POST"])
def reset():
    global chat_history
    chat_history = []
    return jsonify({"status": "history cleared"})


if __name__ == "__main__":
    print("Сервер привида запущено на порту 5000...")
    app.run(port=5000)

    

""" from flask import Flask, request, jsonify
import requests

app = Flask(__name__)

# Тут зберігається історія (можна додати очищення по кнопці)
chat_history = []

OLLAMA_URL = "http://localhost:11434/api/chat"
MODEL = "llama3.2:3b"


@app.route("/ask", methods=["POST"])
def ask():
    global chat_history
    data = request.json
    prompt = data.get("prompt", "").strip()

    if not prompt:
        return jsonify({"error": "Порожній prompt"}), 400

    # Додаємо повідомлення користувача в історію
    chat_history.append({"role": "user", "content": prompt})

    # Формуємо запит до Ollama
    payload = {
        "model": MODEL,
        "messages": chat_history,
        "stream": False,
        "options": {
            "temperature": 0.4,
            "num_predict": 100
            }
    }

    try:
        response = requests.post(OLLAMA_URL, json=payload)
        response.raise_for_status()
        result = response.json()

        # Дістаємо відповідь від асистента
        assistant_message = result["message"]["content"]

        # Додаємо в історію
        chat_history.append({"role": "assistant", "content": assistant_message})

        return jsonify({"response": assistant_message})

    except Exception as e:
        return jsonify({"error": str(e)}), 500


@app.route("/reset", methods=["POST"])
def reset():
    #Очищає історію чату#
    global chat_history
    chat_history = []
    return jsonify({"status": "history cleared"})


if __name__ == "__main__":
    print("Сервер привида запущено на порту 5000...")
    app.run(port=5000)
"""
