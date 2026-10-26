---
trigger: always_on
---

# UI/UX & Design Guidelines (Anti-AI-Slop)

When designing, updating, or styling interfaces in this project:

1. **No Generic AI Slop:**
   - Avoid generic neon purple/cyan gradient glow tropes.
   - Avoid meaningless 3-column bento grids and cards with generic icons.
   - Ban marketing buzzwords ("Supercharge your workflow") in user-facing UI; use direct, human, task-focused microcopy.
2. **Platform-Native Craftsmanship:**
   - Follow Windows Fluent Design patterns: Segoe UI, 8px layout rhythm, Dark Slate/Zinc palette, high-contrast readable text (WCAG AA).
   - Ensure clean microcopy: e.g. "Audio silenciado al cerrar", "Tu equipo sigue trabajando aunque cierres tu dispositivo".
3. **Complete State Coverage:**
   - Every interactive control must account for hover, active/pressed, focus-visible, and disabled states.
   - Prevent focus traps and button re-trigger loops when keyboard keys (Enter/Space) are pressed.
