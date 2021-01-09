---
--- Triggers data table.
---

CREATE TABLE public."Triggers"
(
    "ID" BIGINT NOT NULL GENERATED ALWAYS AS IDENTITY (INCREMENT 1 START 1) CONSTRAINT "PK_Triggers" PRIMARY KEY,
    "SensorID" VARCHAR(24) NOT NULL,
    "KeyValue" VARCHAR(32) NOT NULL,
    "LowerEdge" NUMERIC,
    "UpperEdge" NUMERIC,
    "FormalLanguage" TEXT,
    "Type" INT DEFAULT 0 NOT NULL
);

CREATE INDEX "IX_Triggers_SensorId"
    on "Triggers" ("SensorID");

CREATE INDEX "IX_Triggers_Type"
    on "Triggers" ("Type");
