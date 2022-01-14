#!/usr/bin/env python3
"""Rewrites a Dockerfile, concatenating consecutive "RUN" commands, in order to
minimize the number of layers and produce a more lightweight image
"""
import argparse
import copy
import re
from dataclasses import dataclass
from typing import List

DOCKERFILE = "scraper.Dockerfile"

@dataclass
class Options:
	verbose: bool
	filename: str

def parse() -> Options:
	parser = argparse.ArgumentParser(
		description="Reads from a Dockerfile and writes another Dockerfile to "
		+ "STDOUT, in such a way to condense as many consecutive 'RUN' commands "
		+ "as possible, and thus make Docker produce a more lightweight image."
	)
	parser.add_argument(
		'--verbose',
		'-v',
		'-V',
		action='store_true',
		help="Print statistics for Dockerfile, before and after refactor."
	)
	parser.add_argument(
		'filename',
		type=str,
		help="Name of file to be read."
	)
	args = parser.parse_args()
	return Options(**vars(args))

def tokenize(source: str) -> List[List[str]]:
	lines = source.split("\n")
	commands: List[List[str]] = []

	for line in lines:
		if match := re.search(r"^[A-Z]+", line):
			commands.append([])
		
		commands[-1].append(line)
	return commands

def concatenate_run_commands(old_commands: List[List[str]]) -> List[List[str]]:
	_old_commands_cp = copy.deepcopy(old_commands)
	if not len(old_commands):
		return _old_commands_cp

	# Remove the comments
	old_commands_cp: List[List[str]] = []
	for command in _old_commands_cp:
		old_commands_cp.append([line for line in command if not "#" in line])
	
	new_commands: List[List[str]] = [old_commands_cp[0]]
	get_directive = lambda x: re.split(r"\W+", x[0])[0]

	for i in range(1, len(old_commands)):
		directive_curr = get_directive(old_commands_cp[i])
		directive_last = get_directive(new_commands[-1])
		if directive_curr == directive_last == "RUN":
			old_commands_cp[i][0] = old_commands_cp[i][0].replace("RUN", "\t&&")
			new_commands[-1].extend(old_commands_cp[i])
		else:
			new_commands.append(old_commands_cp[i])

	return new_commands

def stats(commands: List[List[str]]):
	total_lines = sum(len(command) for command in commands)
	n_comments = sum(sum(1 for line in command if "#" in line) for command in commands)
	print(f"File consists of {total_lines} lines, distributed in {len(commands)} commands")
	print(f"Of those lines, {n_comments} are comments.")
	print(f"Average number of non-comment lines per command = {(total_lines - n_comments) / len(commands):.2f}")

def print_commands(commands: List[List[str]], verbose: bool = False):
	for command in commands:
		if verbose:
			print('-----')
		for line in command:
			print(line)

def main():
	options = parse()
	if options.verbose:
		print("Running in verbose mode...")

	with open(options.filename, "r") as fp:
		source = fp.read()
	old_commands = tokenize(source)

	new_commands = concatenate_run_commands(old_commands)
	print_commands(new_commands, options.verbose)

	if options.verbose:
		print("-----")
		stats(old_commands)
		stats(new_commands)

if __name__ == "__main__":
	main()