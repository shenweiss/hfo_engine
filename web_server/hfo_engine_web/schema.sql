
DROP TABLE IF EXISTS ch_name_translation;

CREATE TABLE ch_name_translation (
  long_name TEXT PRIMARY KEY,
  short_name TEXT NOT NULL
);