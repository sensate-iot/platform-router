---
--- Live data handler table.
---

CREATE TABLE public."LiveDataHandlers"
(
    "ID" BIGINT NOT NULL GENERATED ALWAYS AS IDENTITY (INCREMENT 1 START 1) CONSTRAINT "PK_LiveDataHandlers" PRIMARY KEY,
    "Name" VARCHAR(64) NOT NULL UNIQUE,
    "Enabled" BOOLEAN NOT NULL
);

CREATE INDEX "IX_LiveDataHandlers_Enabled"
    ON "LiveDataHandlers" ("Enabled");
