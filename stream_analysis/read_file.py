import pandas as pd
import matplotlib.pyplot as plt
import json

# Function to parse the JSONL file into a DataFrame
def parse_jsonl_to_dataframe(jsonl_file_path: str):
    records = []
    with open(jsonl_file_path, 'r') as file:
        for line in file:
            record = json.loads(line)
            # Extract the time_boot_ms (time since boot) and received_seconds_since_start
            time_since_boot_ms = record["message"]["time_boot_ms"] / 1000.0  # Convert ms to seconds
            received_seconds_since_start = record["received_seconds_since_start"]
            records.append({"time_since_boot_sec": time_since_boot_ms, "received_seconds_since_start": received_seconds_since_start})
    return pd.DataFrame(records)

# Function to plot the DataFrame
def plot_data(df):
    plt.figure(figsize=(12, 6))
    plt.plot(df["received_seconds_since_start"], df["time_since_boot_sec"], label="Time Since Boot (sec)", marker='o', linestyle='-')
    plt.xlabel("Time Received (seconds since start)")
    plt.ylabel("Time Since Boot (seconds)")
    plt.title("Time Since Boot vs. Time Received")
    plt.legend()
    plt.grid(True)
    plt.show()

if __name__ == "__main__":
    jsonl_file_path = "websocket_data.json"  # Update this path to your JSON Lines file location
    df = parse_jsonl_to_dataframe(jsonl_file_path)
    plot_data(df)
