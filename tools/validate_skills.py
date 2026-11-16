#!/usr/bin/env python3
"""Validate repository skill frontmatter without third-party packages."""

from __future__ import annotations

import re
import sys
from pathlib import Path


ALLOWED_KEYS = {"name", "description", "license", "allowed-tools", "metadata"}
NAME_RE = re.compile(r"^[a-z0-9]+(?:-[a-z0-9]+)*$")
FRONTMATTER_RE = re.compile(r"^---\n(.*?)\n---(?:\n|$)", re.DOTALL)


def read_top_level_fields(frontmatter: str) -> dict[str, str]:
    fields: dict[str, str] = {}
    for line in frontmatter.splitlines():
        if not line or line[0].isspace():
            continue
        key, separator, value = line.partition(":")
        if not separator:
            raise ValueError(f"invalid top-level frontmatter line: {line}")
        fields[key.strip()] = value.strip().strip('"\'')
    return fields


def validate_skill(skill_dir: Path) -> list[str]:
    skill_file = skill_dir / "SKILL.md"
    if not skill_file.is_file():
        return ["SKILL.md not found"]

    content = skill_file.read_text(encoding="utf-8")
    match = FRONTMATTER_RE.match(content)
    if not match:
        return ["invalid or missing YAML frontmatter"]

    try:
        fields = read_top_level_fields(match.group(1))
    except ValueError as error:
        return [str(error)]

    errors: list[str] = []
    unexpected = sorted(set(fields) - ALLOWED_KEYS)
    if unexpected:
        errors.append("unexpected frontmatter keys: " + ", ".join(unexpected))

    name = fields.get("name", "")
    if not NAME_RE.fullmatch(name) or len(name) > 64:
        errors.append("name must be hyphen-case and at most 64 characters")

    description = fields.get("description", "")
    if not description:
        errors.append("description is required")
    elif len(description) > 1024 or "<" in description or ">" in description:
        errors.append("description must be at most 1024 characters without angle brackets")

    if re.search(r"^ {0,3}\[TODO:[^\n]*\]\s*$", content, re.MULTILINE):
        errors.append("unfinished TODO placeholder")
    return errors


def skill_directories(paths: list[str]) -> list[Path]:
    if paths:
        return [Path(path) for path in paths]
    return sorted(path.parent for path in Path(".claude/skills").glob("*/SKILL.md"))


def main() -> int:
    failed = False
    for skill_dir in skill_directories(sys.argv[1:]):
        errors = validate_skill(skill_dir)
        if errors:
            failed = True
            print(f"{skill_dir}: " + "; ".join(errors))
        else:
            print(f"{skill_dir}: valid")
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main())
