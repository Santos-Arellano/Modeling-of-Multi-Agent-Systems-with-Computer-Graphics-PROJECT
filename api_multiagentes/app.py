from flask import Flask, request, jsonify

app = Flask(__name__)
@app.route('/')
def index():
    return jsonify(200)
@app.route('/receive_positions', methods=['POST'])
def receive_positions():
    data = request.json
    print("Received positions:", data)
    # Process data, potentially modify it, and send back a response if needed

    # Example: Send back a confirmation message
    return jsonify({"status": "success", "received_data": data})

if __name__ == '__main__':
    app.run(debug=True, port=5000)
