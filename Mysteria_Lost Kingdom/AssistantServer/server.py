from flask import Flask, request, jsonify
import requests
import json

app = Flask(__name__)

OLLAMA_URL = "http://localhost:11434/api/chat"
MODEL = "qwen2.5:3b"

MAX_HISTORY = 4

SYSTEM_PROMPT = {
    "role": "system",
    "content": (
        "You are the Ghost, an ancient spirit from the fallen kingdom of Illyria "
        "on the continent of Mysteria.\n\n"

        "Your role is to guide the player.\n"
        "You speak calmly and mysteriously, but you must always give correct and useful information.\n\n"

        "If you don't have enough information, ask the player for clarification.\n"
        "Do NOT invent player stats, quest data or other info about player.\n\n"

        "Rules:\n"
        "- Never invent facts.\n"
        "- If the player greets you, greet them briefly.\n"
        "- If the player asks about mathematics, science, or knowledge, explain it correctly but in a mystical tone.\n"
        "- Keep answers short (1-3 sentences).\n"
        "- Do not start long stories unless the player asks for lore.\n"
        "- If you do not know something, say you do not know.\n\n"

        "When answering educational questions:\n"
        "- First check if the player likely knows the topic\n"
        "- If weak → explain step-by-step\n"
        "- If strong → give hint instead of full answer\n"
        "- Encourage thinking instead of giving full solution immediately\n\n"

        "NPC emotional system:\n"
        "You must analyze how the player speaks to you and adjust your attitude.\n"
        "Return your answer ONLY in JSON format with the structure:\n"
        "- response must always contain at least 1 short sentence\n"
        "- NEVER return empty response\n"

        "{\n"
        " response: string,\n"
        " trust_change: number (-0.1 to 0.1),\n"
        " mood_change: number (-0.1 to 0.1),\n"
        " mystery_change: number (-0.05 to 0.05)\n"
        "}\n\n"

        "Behavior rules:\n"
        "- Polite player → increase trust\n"
        "- Rude player → decrease trust\n"
        "- Friendly player → increase mood\n"
        "- Hostile or aggressive language → decrease mood\n"
        "- Player asks a question → decrease mystery\n"
        "- Simple question (basic, obvious, everyday topics) → small mystery decrease (-0.01 to -0.02)\n"
        "- Complex or deep question (lore, secrets, философські, складні теми) → larger mystery decrease (-0.03 to -0.05)\n"
        "- If nothing important happens return 0 changes\n\n"

        "Emotional interpretation rules:\n"
        "- Trust close to 1 → you trust the player and speak openly\n"
        "- Trust close to 0 → you distrust the player and may be cold or distant\n"

        "- Mood close to 1 → you are friendly, warm, and engaged\n"
        "- Mood close to 0 → you are cold, irritated, or reluctant\n"

        "- Mystery close to 1 → you speak vaguely and mysteriously\n"
        "- Mystery close to 0 → you give more direct and clear answers\n"

        "You MUST adjust your tone based on these values.\n"
    )
}

chat_history = []


# NPC behaviour state (для майбутніх формул)
npc_state = {
    "trust": 0.5,
    "mood": 0.5,
    "mystery": 0.8
}

memory_summary = ""


def trim_history():
    global chat_history
    if len(chat_history) > MAX_HISTORY:
        chat_history = chat_history[-MAX_HISTORY:]

def update_memory():
    global memory_summary, chat_history

    summary_prompt = [
        {
            "role": "system",
            "content": (
                "Summarize the interaction briefly.\n"
                "Focus on:\n"
                "- player attitude\n"
                "- topics discussed\n"
                "- relationship evolution\n"
                "Keep it under 2 sentences."
            )
        }
    ]

    summary_prompt.extend(chat_history)

    payload = {
        "model": MODEL,
        "messages": summary_prompt,
        "stream": False,
        "options": {
            "temperature": 0.2,
            "num_predict": 60
        }
    }

    try:
        r = requests.post(OLLAMA_URL, json=payload)
        r.raise_for_status()

        result = r.json()
        summary = result.get("message", {}).get("content", "")

        if summary:
            memory_summary = summary

    except Exception as e:
        print("[MEMORY ERROR]", e)
        

def build_prompt(user_prompt):

    messages = [SYSTEM_PROMPT]

    npc_context = (
        f"Ghost emotional state:\n"
        f"Trust to player: {npc_state['trust']}\n"
        f"Mood: {npc_state['mood']}\n"
        f"Mystery level: {npc_state['mystery']}\n"
    )

    messages.append({"role": "system", "content": npc_context})

    if memory_summary:
        messages.append({
            "role": "system",
            "content": f"Memory of past interactions:\n{memory_summary}"
        })

    messages.extend(chat_history)

    messages.append({
        "role": "user",
        "content": user_prompt
    })

    return messages


@app.route("/ask", methods=["POST"])
def ask():
    global chat_history
    global npc_state

    data = request.json

    prompt = data.get("prompt", "").strip()
    stats = data.get("ghostStats", {})

    if not prompt:
        return jsonify({"response": "Empty prompt"}), 400

    # беремо стан NPC з Unity
    npc_state["trust"] = stats.get("trust", npc_state["trust"])
    npc_state["mood"] = stats.get("mood", npc_state["mood"])
    npc_state["mystery"] = stats.get("mystery", npc_state["mystery"])

    messages = build_prompt(prompt)

    payload = {
        "model": MODEL,
        "messages": messages,
        "stream": False,
        "format": "json",
        "options": {
            "temperature": 0.2,
            "num_predict": 120,
            "top_p": 0.85,
            "repeat_penalty": 1.2,
            "num_ctx": 1024
        }
    }

    try:
        r = requests.post(OLLAMA_URL, json=payload)
        r.raise_for_status()

        resp_json = r.json()
        ai_text = resp_json.get("message", {}).get("content", "")

        # ---- Виправлений парсинг JSON від моделі ----
        try:
            # шукаємо блок JSON всередині відповіді
            start = ai_text.find("{")
            end = ai_text.rfind("}")
            
            if start != -1 and end != -1:
                json_block = ai_text[start:end+1]
                ai_json = json.loads(json_block)
            else:
                # якщо JSON не знайдено
                ai_json = {}

            response_text = ai_json.get("response", "")
            trust_delta = float(ai_json.get("trust_change", 0))
            mood_delta = float(ai_json.get("mood_change", 0))
            mystery_delta = float(ai_json.get("mystery_change", 0))

        except Exception as e:
            print(f"[WARNING] Failed to parse AI JSON: {e}, raw text: {ai_text}")
            response_text = "Дух мовчить... Щось порушує зв'язок..."
            trust_delta = 0
            mood_delta = 0
            mystery_delta = 0
            
        # оновлюємо стан NPC
        npc_state["trust"] = min(max(npc_state["trust"] + trust_delta, 0), 1)
        npc_state["mood"] = min(max(npc_state["mood"] + mood_delta, 0), 1)
        npc_state["mystery"] = min(max(npc_state["mystery"] + mystery_delta, 0), 1)

        chat_history.append({"role": "user", "content": prompt})
        chat_history.append({"role": "assistant", "content": response_text})

        trim_history()

        if len(chat_history) >= 4:
            update_memory()
            chat_history = []

        # повертаємо вже чистий JSON з числами та текстом
        return jsonify({
            "response": response_text,
            "trust_change": trust_delta,
            "mood_change": mood_delta,
            "mystery_change": mystery_delta
        })

    except Exception as e:
        return jsonify({"response": f"[ERROR]: {str(e)}"}), 500


@app.route("/reset", methods=["POST"])
def reset():

    global chat_history

    chat_history = []

    npc_state["trust"] = 0.5
    npc_state["mood"] = 0.5
    npc_state["mystery"] = 0.8

    return jsonify({"status": "reset"})


if __name__ == "__main__":

    print("Ghost AI server running on port 5000...")
    app.run(port=5000)

