from flask import Flask, request, jsonify

app = Flask(__name__)

@app.route('/', methods=['POST'])
def receive_positions():
    data = request.get_json()
    print("Received data:", data)
    return jsonify({"status": "success"}), 200

if __name__ == '__main__':
    app.run(debug=True)
