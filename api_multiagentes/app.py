from flask import Flask, request, jsonify
from constants import positions
from agent import start


app = Flask(__name__)


@app.route('/move', methods=['POST'])
def move_object():
    global positions
    print(positions)
    # positions = []
    start()
    return jsonify({'status': 'success'})

@app.route('/move', methods=['GET'])
def get_position():
    return jsonify(positions.pop(0))

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)