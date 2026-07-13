"""Seed realistic demo data into LearnFlow through the API (same write path as the app).

Creates 4 topics, 6 folders, 12 documents, 8 notes, and ~78 study blocks
(a hand-crafted current week plus ~9 weeks of history for the activity heatmap).

Usage:
    1. Start the DB and the API:   docker compose up -d db && dotnet run
    2. Run seed.sql first (admin user + permissions) if the DB is fresh.
    3. python3 seed_demo.py

The script APPENDS data — to re-seed from scratch, wipe the domain tables first:
    docker exec learnflow-db psql -U learnflow -d learnflow \
      -c 'TRUNCATE "Topics", "TopicFolders", "Documents", "Notes", "StudyBlocks" CASCADE;'
"""
import json
import urllib.request
from datetime import datetime, timedelta, timezone

BASE = "http://localhost:5068/v1"
NOW = datetime.now(timezone.utc)
# Local midnight, so seeded block hours read naturally in the user's timezone.
TODAY = NOW.astimezone().replace(hour=0, minute=0, second=0, microsecond=0)


def call(method, path, body=None, token=None):
    req = urllib.request.Request(f"{BASE}{path}".replace(" ", "%20"), method=method)
    req.add_header("Content-Type", "application/json")
    if token:
        req.add_header("Authorization", f"Bearer {token}")
    data = json.dumps(body).encode() if body is not None else None
    try:
        with urllib.request.urlopen(req, data=data) as resp:
            raw = resp.read()
            return json.loads(raw) if raw else None
    except urllib.error.HTTPError as e:
        print(f"  !! {method} {path} -> {e.code}: {e.read().decode()[:200]}")
        return None


def iso(dt):
    return dt.astimezone(timezone.utc).isoformat().replace("+00:00", "Z")


token = call("POST", "/Auth/login", {"email": "admin@learnflow.local", "password": "admin123"})["token"]
print("logged in")

# --- clean up earlier smoke-test data ---------------------------------------
existing = call("GET", "/Topics?$filter=Description eq 'Smoke test topic'", token=token)
for t in existing["value"]:
    call("DELETE", f"/Topics({t['Id']})", token=token)
blocks = call("GET", "/StudyBlocks?$filter=Title eq 'Smoke test block'", token=token)
for b in blocks["value"]:
    call("DELETE", f"/StudyBlocks({b['Id']})", token=token)
print("cleaned smoke-test leftovers")

# --- topics ------------------------------------------------------------------
topics_spec = [
    ("Machine Learning", "Andrew Ng course + hands-on projects", "#6366f1"),
    ("Japanese N3", "JLPT N3 preparation — grammar, vocab, listening", "#0284c7"),
    ("System Design", "Interview prep: scalability, databases, caching", "#059669"),
    ("UI/UX Design", "Design fundamentals and Figma practice", "#d97706"),
]
topics = {}
for title, desc, color in topics_spec:
    t = call("POST", "/Topics", {"Title": title, "Description": desc, "Color": color}, token)
    topics[title] = t["Id"]
print(f"created {len(topics)} topics")

# --- folders -----------------------------------------------------------------
folders_spec = [
    ("Machine Learning", "Course materials"),
    ("Machine Learning", "Papers"),
    ("Japanese N3", "Grammar"),
    ("Japanese N3", "Listening practice"),
    ("System Design", "Case studies"),
    ("UI/UX Design", "References"),
]
folders = {}
for topic, name in folders_spec:
    f = call("POST", "/TopicFolders", {"TopicId": topics[topic], "Name": name}, token)
    folders[(topic, name)] = f["Id"]
print(f"created {len(folders)} folders")

# --- documents ---------------------------------------------------------------
docs_spec = [
    # topic, folder, title, type, status, minutes, days_ago_opened
    ("Machine Learning", "Course materials", "Week 1 — Intro to ML.pdf", "PDF", "Completed", 95, 6),
    ("Machine Learning", "Course materials", "Week 2 — Linear Regression.pdf", "PDF", "Completed", 120, 4),
    ("Machine Learning", "Course materials", "Week 3 — Logistic Regression.pdf", "PDF", "InProgress", 45, 1),
    ("Machine Learning", "Papers", "Attention Is All You Need", "Link", "Unread", 0, None),
    ("Japanese N3", "Grammar", "N3 Grammar list — てしまう / ばかり", "Markdown", "Completed", 60, 3),
    ("Japanese N3", "Grammar", "Shin Kanzen Master — Ch. 5", "PDF", "InProgress", 30, 1),
    ("Japanese N3", "Listening practice", "JLPT N3 listening drills vol.2", "Video", "Unread", 0, None),
    ("System Design", "Case studies", "Designing a URL shortener", "Link", "Completed", 40, 5),
    ("System Design", "Case studies", "Rate limiter patterns", "Markdown", "InProgress", 25, 2),
    ("System Design", None, "DDIA — Chapter 6 (Partitioning)", "PDF", "Unread", 0, None),
    ("UI/UX Design", "References", "Refactoring UI — notes", "PDF", "Completed", 80, 7),
    ("UI/UX Design", None, "Laws of UX cheat sheet", "Link", "Unread", 0, None),
]
documents = {}
for topic, folder, title, ftype, status, minutes, opened in docs_spec:
    body = {
        "TopicId": topics[topic],
        "Title": title,
        "FileType": ftype,
        "Status": status,
        "TimeSpentMinutes": minutes,
    }
    if folder:
        body["FolderId"] = folders[(topic, folder)]
    if opened is not None:
        body["LastOpenedAt"] = iso(NOW - timedelta(days=opened))
    d = call("POST", "/Documents", body, token)
    documents[title] = d["Id"]
print(f"created {len(documents)} documents")

# --- notes ---------------------------------------------------------------
notes_spec = [
    # topic, doc, title, content, days_ago_updated
    ("Machine Learning", "Week 2 — Linear Regression.pdf", "Gradient descent intuition",
     "# Gradient descent\n\n- Step proportional to negative gradient\n- Learning rate too high → divergence\n- Feature scaling speeds up convergence\n\n**TODO**: implement from scratch in numpy", 4),
    ("Machine Learning", "Week 3 — Logistic Regression.pdf", "Sigmoid & decision boundary",
     "Sigmoid squashes to (0,1).\n\nDecision boundary is *linear* in feature space even though the output is nonlinear.\n\nCross-entropy loss, not MSE — MSE is non-convex here.", 1),
    ("Machine Learning", None, "Project ideas",
     "1. Price prediction on local housing data\n2. JLPT vocab difficulty classifier (ties into Japanese!)\n3. Study-time forecaster from LearnFlow analytics", 2),
    ("Japanese N3", "N3 Grammar list — てしまう / ばかり", "てしまう nuances",
     "- 完了 (completion): 読んでしまった = finished reading\n- 後悔 (regret): 忘れてしまった = accidentally forgot\n- Casual: 〜ちゃう / 〜じゃう", 3),
    ("Japanese N3", None, "Vocab — week 28",
     "| 単語 | 読み | 意味 |\n|---|---|---|\n| 締め切り | しめきり | deadline |\n| 予定 | よてい | plan |\n| 復習 | ふくしゅう | review |", 0),
    ("System Design", "Designing a URL shortener", "URL shortener — key points",
     "- Base62 over auto-increment ID\n- 301 vs 302: use 302 to keep analytics\n- Cache hot URLs (80/20), Redis with LRU\n- Estimate: 100:1 read/write ratio", 5),
    ("System Design", "Rate limiter patterns", "Token bucket vs sliding window",
     "Token bucket: allows bursts, simple.\nSliding window log: accurate, memory heavy.\nSliding window counter: good compromise.\n\nRedis + Lua for atomicity.", 2),
    ("UI/UX Design", "Refactoring UI — notes", "Visual hierarchy rules",
     "- Don't rely on font size alone — use weight and color\n- Labels are ink: de-emphasize them\n- White space before borders", 6),
]
for topic, doc, title, content, updated in notes_spec:
    body = {
        "TopicId": topics[topic],
        "Title": title,
        "Content": content,
        "CreatedAt": iso(NOW - timedelta(days=updated, hours=2)),
        "UpdatedAt": iso(NOW - timedelta(days=updated)),
    }
    if doc:
        body["DocumentId"] = documents[doc]
    call("POST", "/Notes", body, token)
print(f"created {len(notes_spec)} notes")

# --- study blocks -----------------------------------------------------------
def block(topic, title, day_offset, start_h, start_m, dur_min, status, reminder=5):
    start = TODAY + timedelta(days=day_offset, hours=start_h, minutes=start_m)
    return {
        "TopicId": topics[topic] if topic else None,
        "Title": title,
        "StartAt": iso(start),
        "EndAt": iso(start + timedelta(minutes=dur_min)),
        "Status": status,
        "ReminderMinutesBefore": reminder,
    }

blocks_spec = [
    # past week — mostly Done, feeds weekly-focus chart, streak and adherence
    block("Machine Learning", "ML Week 1 wrap-up", -6, 9, 0, 90, "Done"),
    block("Japanese N3", "Grammar drills", -6, 19, 0, 45, "Done"),
    block("UI/UX Design", "Refactoring UI reading", -5, 20, 0, 60, "Done"),
    block("System Design", "URL shortener case study", -5, 9, 30, 60, "Done"),
    block("Machine Learning", "Linear regression lab", -4, 9, 0, 120, "Done"),
    block("Japanese N3", "Listening practice", -4, 19, 0, 30, "Missed"),
    block("Japanese N3", "N3 vocab review", -3, 19, 0, 45, "Done"),
    block("System Design", "Rate limiter deep dive", -2, 10, 0, 60, "Done"),
    block("Machine Learning", "Logistic regression lecture", -1, 9, 0, 90, "Done"),
    block("Japanese N3", "Kanji writing", -1, 21, 0, 30, "Done"),
]

# today — one done this morning, one starting ~20 min from now (alert demo), one tonight
now_local_min = (NOW - TODAY).total_seconds() / 60
blocks_spec.append(block("Machine Learning", "ML — logistic regression lab", 0, 8, 30, 60, "Done"))
soon = NOW + timedelta(minutes=20)
blocks_spec.append({
    "TopicId": topics["System Design"],
    "Title": "System design mock interview",
    "StartAt": iso(soon),
    "EndAt": iso(soon + timedelta(minutes=60)),
    "Status": "Upcoming",
    "ReminderMinutesBefore": 10,
})
blocks_spec.append(block("Japanese N3", "Evening listening drills", 0, 21, 0, 30, "Upcoming"))

# tomorrow's plan
blocks_spec.append(block("Machine Learning", "Start Week 4 — Neural nets", 1, 9, 0, 90, "Upcoming"))
blocks_spec.append(block("Japanese N3", "Grammar — Shin Kanzen Ch. 6", 1, 19, 0, 45, "Upcoming"))
blocks_spec.append(block("UI/UX Design", "Figma practice — dashboard redesign", 1, 15, 0, 60, "Upcoming"))

# --- history for the activity heatmap (day -70 .. day -7) --------------------
import random

random.seed(7)
topic_names = list(topics.keys())
history_titles = {
    "Machine Learning": ["ML lecture", "ML lab", "Kaggle practice"],
    "Japanese N3": ["Grammar drills", "Vocab review", "Listening practice"],
    "System Design": ["Case study", "DDIA reading", "Mock interview prep"],
    "UI/UX Design": ["Design reading", "Figma practice", "UI critique"],
}
history = []
for day in range(-70, -6):
    if random.random() > 0.68:
        continue  # rest day
    for _ in range(random.choice([1, 1, 2])):
        topic = random.choice(topic_names)
        start_h = random.choice([8, 9, 10, 14, 19, 20, 21])
        dur = random.choice([30, 45, 60, 90, 120])
        history.append(
            block(topic, random.choice(history_titles[topic]), day, start_h, 0, dur, "Done")
        )
blocks_spec.extend(history)

for b in blocks_spec:
    call("POST", "/StudyBlocks", b, token)
print(f"created {len(blocks_spec)} study blocks ({len(history)} historical)")

# --- summary -----------------------------------------------------------------
for entity in ["Topics", "TopicFolders", "Documents", "Notes", "StudyBlocks"]:
    count = call("GET", f"/{entity}?$count=true&$top=1", token=token)["@odata.count"]
    print(f"{entity}: {count}")
