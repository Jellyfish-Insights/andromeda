from difflib import SequenceMatcher
from typing import List, Tuple


managed_account = "rosalindsieger4824@gmail.com"

candidates = ["Rosalind Sieger", "Charlie Brown", "Ronald Reagan", "Lex Luthor"]
longest_matches: List[Tuple[str, int]] = []
for title in candidates:
	s = SequenceMatcher(None, managed_account, title.lower())
	longest_matches.append((title, s.find_longest_match(0, len(managed_account), 0, len(title)).size))

print(longest_matches)
best_title, best_score = max(longest_matches, key=lambda x: x[1])
print(best_title, best_score)