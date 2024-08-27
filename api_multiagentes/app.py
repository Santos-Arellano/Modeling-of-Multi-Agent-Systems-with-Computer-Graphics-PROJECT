from flask import Flask, request, jsonify
from constants import positions
from agent import start


app = Flask(__name__)


@app.route('/move', methods=['POST'])
def move_object():
    global positions
    start()
    return jsonify({'status': 'success'})

@app.route('/move', methods=['GET'])
def get_position():
    if positions:
        return jsonify(positions.pop(0))
    else:
        return jsonify({"error": "No positions available"})
    
@app.route('/move', methods=['DELETE'])
def reset_positions():
    global positions
    positions = []
    return jsonify({'status': 'success'})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)