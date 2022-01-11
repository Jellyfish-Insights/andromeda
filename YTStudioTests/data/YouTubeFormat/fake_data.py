import secrets
import random
import time
import sys
from itertools import combinations

VIDEOS_CHANNELS = {secrets.token_hex(8): secrets.token_hex(4) for _ in range(1000)}
METRICS = ["views", "impressions", "subscribers gained", "likes", "shares"]

def random_csv_data(rows: int):
    # Determining metrics for header
    number_of_metrics = random.randint(1, len(METRICS))
    metrics_chosen = random.choice(list(combinations(METRICS, number_of_metrics)))
    csv_header = [f"Date Measure,Video ID,Channel ID,{','.join(metrics_chosen)}"]
    csv_rows = []
    for _ in range(rows):
        # A date up to 5 years ago
        MAX_SECONDS_AGO = 60 * 60 * 24 * 365 * 5
        date_measure = int(time.time()) - random.randint(0, MAX_SECONDS_AGO)
        video_id, channel_id = random.choice(list(VIDEOS_CHANNELS.items()))
        random_metrics = []
        for m in metrics_chosen:
            random_metrics.append(random.randint(0, 1000))
        csv_rows.append(f"{date_measure},{video_id},{channel_id},{','.join([str(x) for x in random_metrics])}")
    print('\n'.join(csv_header + csv_rows))

def main():
    if len(sys.argv != 1):
        print(f"Usage: {__file__} <NUMBER_OF_ROWS>")
        return
    random_csv_data(int(sys.argv[1]))

if __name__ == "__main__":
    main()
