# Latest character expressions

Generated with the built-in image-generation tool from the six supplied character models. These are project-ready sprite atlases with genuine PNG alpha; the original expression images and existing `.meta` files remain intact.

## Assets and expressions

| Atlas | Layout and order | Pixels per unit |
| --- | --- | --- |
| `jane_expressions_latest.png` | 4 × 1; curious, thinking, surprised, sad | 7.61324705 |
| `paul_expressions_latest.png` | 4 × 1; smug, angry, shocked, defensive | 6.83487102 |
| `shan_expressions_latest.png` | 2 × 2; thinking, curious, sad, nervous | 5.77395728 |
| `shanaia_expressions_latest.png` | 2 × 2; nervous, angry, shocked, defensive | 6.14788437 |
| `vince_expressions_latest.png` | 2 × 2; defensive, nervous, shocked, angry | 5.21322812 |
| `vonn_expressions_latest.png` | 3 × 1; defensive, nervous, shocked | 6.08355091 |

Order is left to right, then top to bottom. Vonn is the character stored under `CHAR_CHARL_PASCUAL` (Charl Vonn Pascual); Shania uses the existing `CHAR_SHANAIA_ORTEGA` ID. Names and enum values are preserved.

The import rectangles use a common crop across each character's poses. Pixels per unit are calibrated against the corresponding BAGO neutral sprite's authored height. All 23 existing non-neutral entries in `Assets/Resources/CharacterExpressions.asset` now use these atlases. Vince's three profile-level overrides were updated as well because those take precedence over the library. Neutral sprites, dialogue, scenes, gameplay code, and scoring are unchanged.

## Final prompt set

All prompts used the identity-preserve workflow: use the supplied latest model as the exact identity, wardrobe, pose and style reference; change only facial expression; keep consistent framing, head position and scale; output genuinely transparent PNG cutouts without scenery, labels, grids, text, shadows or watermark. Preserve clean anime linework and the reference's cel shading. Leave separate cells with complete hair, shoulders and arms.

### Jane

Reference: `Assets/Assets/CHARACTERS/BAGO JANE.png` (supplied image 1). Four equal horizontal cells: curious, thinking, surprised, sad. Long dark brown hair and wispy bangs, black-framed glasses, warm light skin and brown eyes, black sleeveless high-neck halter top, black jeans. Curious: questioning raised eyebrow and attentive eyes. Thinking: pensive furrow and sideways glance. Surprised: wide eyes, raised brows, small open mouth. Sad: lifted inner brows, downturned lips and downcast eyes. Preserve front-facing waist-up pose. Do not substitute a white shirt or change hair length.

### Paul

Reference: `Assets/Assets/CHARACTERS/BAGO PAUL.png` (supplied image 2). Four equal horizontal cells: smug, angry, shocked, defensive. Short swept black hair, thick brows, warm tan skin, no glasses, white short-sleeve button shirt with red inner collar and red breast-pocket trim, hands in trouser pockets. Smug: one-sided smirk, half-lidded confident eyes. Angry: lowered knitted brows, narrowed eyes, tense mouth. Shocked: wide eyes, raised brows, open mouth. Defensive: tense raised inner brows, anxious skeptical eyes and tight lips. A background-extraction follow-up removed the first pass's gray haze while preserving opaque white fabric and all four figures.

### Shan

Reference: `Assets/Assets/CHARACTERS/BAGO SHAN.png` (supplied image 3). Two-by-two equal square cells: top left thinking, top right curious, bottom left sad, bottom right nervous. Shoulder-length straight center-parted dark brown hair, black rectangular glasses, brown eyes, fair warm skin, loose off-white crew-neck T-shirt. Preserve full head, shoulders and sleeves. Thinking: pensive brows, sideways glance and closed lips. Curious: one raised brow, attentive eyes and slightly parted lips. Sad: downcast eyes, lifted inner brows and downturned lips. Nervous: uneasy eyes, tense brows and uncertain tight mouth. The final square layout replaces the crowded horizontal draft.

### Shania

Reference: `Assets/Assets/CHARACTERS/BAGO SHANIA.png` (supplied image 4). Two-by-two equal square cells: top left nervous, top right angry, bottom left shocked, bottom right defensive. Red/auburn shoulder-length layered shag hair and bangs, thick black rectangular glasses, warm tan skin, green collared long-sleeve blouse. Preserve the reference's slight head tilt and front-facing torso. Nervous: uneasy eyes, tense raised brows, tight lips. Angry: lowered brows toward nose, narrowed eyes and tense mouth. Shocked: raised brows, wide eyes and small open mouth. Defensive: skeptical raised brow, guarded eyes and firmly pressed lips. No outfit or hair recoloring.

### Vince

Reference: `Assets/Assets/CHARACTERS/BAGO VINCE.png` (supplied image 5). Two-by-two equal square cells: top left defensive, top right nervous, bottom left shocked, bottom right angry. Dense short black curly hair, black rectangular glasses, blue-gray eyes, warm light tan skin, cream long-sleeve button-up shirt, dark brown suspenders, brown belted trousers. Defensive: guarded skeptical eyes, lifted inner brows, tense mouth. Nervous: uneasy sidelong glance, worried raised brows, strained lips. Shocked: wide eyes, raised brows, slightly open mouth. Angry: furrowed lowered brows, narrowed eyes and tense scowl. No tie, straight hair, or wardrobe changes.

### Vonn

Reference: `Assets/Assets/CHARACTERS/BAGO VONN.png` (supplied image 6). Three equal horizontal cells: defensive, nervous, shocked. Short side-parted dark gray-black hair, large round dark glasses, blue-gray eyes, light tan skin, blue short-sleeve polo, dark trousers, brown belt with silver rectangular buckle. Defensive: guarded skeptical gaze, slightly drawn brows, firmly closed mouth. Nervous: worried raised brows, uneasy eyes, tense lips. Shocked: raised brows, wide eyes and visibly open mouth. Preserve the same waist-up body pose and silhouette; no suspenders, curly hair, or white shirt.

## Verification

The selected sheets were visually reviewed against all six supplied models and composited over a colored background to check transparency and silhouette edges. The adjacent preview shows all 23 expressions. New `.meta` files define stable sprite names and local IDs; existing GUIDs and source assets were preserved. Static checks passed for all six RGBA images, all 23 slice bounds and expression enum/reference pairs, Vince's profile overrides, and preservation of the original metadata. No scene or script files changed. Pixels per unit were calculated from the authored neutral heights; live visual scale and import checks remain unverified because the Unity MCP server had no connected editor.
