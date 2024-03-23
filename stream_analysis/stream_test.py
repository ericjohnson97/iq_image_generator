import asyncio
import websockets
import json
import datetime
from typing import Any, Dict

async def log_json_data_from_websocket(uri: str, json_logfile: str):
    """
    Connect to a WebSocket and log received JSON data to a file in JSON Lines format,
    including the time elapsed in seconds since the capture started when each message is received.

    Args:
        uri (str): The WebSocket server URI to connect to.
        json_logfile (str): The path to the JSON file where data should be logged.

    """
    # Capture the start time when the connection is first established
    start_time = datetime.datetime.utcnow()
    async with websockets.connect(uri) as websocket:
        while True:
            message = await websocket.recv()
            data: Dict[str, Any] = json.loads(message)  # Parse JSON string to Python dict
            
            # Calculate the time elapsed in seconds since the start time
            current_time = datetime.datetime.utcnow()
            elapsed_time = (current_time - start_time).total_seconds()
            
            # Add the elapsed time to the data
            data["received_seconds_since_start"] = elapsed_time
            
            # Append the received data, now including the elapsed time, to the file as a JSON string
            with open(json_logfile, "a") as file:
                json.dump(data, file)
                file.write('\n')  # Newline for JSON Lines format
            print(data)  # Optional: Print the received data to the console

if __name__ == "__main__":
    # WebSocket server URI
    # uri = "wss://sim.intelligentquads.com/1d3c3fce5bfa43fb8dd092b8249ca8c9/ws/mavlink?filter=LOCAL_POSITION_NED"
    uri = "ws://192.168.1.222:6040/ws/mavlink?filter=LOCAL_POSITION_NED"
    # Path to the logfile
    json_logfile = "websocket_data.json"
    
    # Start the logging task
    asyncio.run(log_json_data_from_websocket(uri, json_logfile))
