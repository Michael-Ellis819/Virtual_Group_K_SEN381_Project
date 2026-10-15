"""Fail the gate on a reported high/critical advisory or an incomplete scan."""
import json
import sys

with open(sys.argv[1], encoding="utf-8-sig") as report_file:
    report = json.load(report_file)
blocked = []

def visit(value):
    if isinstance(value, dict):
        if str(value.get("severity", "")).lower() in {"high", "critical"}:
            blocked.append("High/critical advisory: " + str(value.get("advisoryurl", value.get("advisoryUrl", "see report"))))
        for key, item in value.items():
            if key.lower() in {"problems", "errors"} and item:
                blocked.append("Incomplete dependency scan: " + str(item))
            visit(item)
    elif isinstance(value, list):
        for item in value:
            visit(item)

if not report.get("sources") or not report.get("projects"):
    blocked.append("Dependency scan has no source/project evidence.")
visit(report)
for reason in blocked:
    print(reason)
if blocked:
    sys.exit(1)
print("PASS dependency gate: no reported high/critical advisory; review lower-severity findings in the report.")
