---
--- Trigger invocations table.
---

CREATE TABLE "TriggerInvocations"
(
    "ID"  BIGINT NOT NULL GENERATED ALWAYS AS IDENTITY (INCREMENT 1 START 1)
        CONSTRAINT "PK_TriggerInvocations" PRIMARY KEY,
    "TriggerID" BIGINT NOT NULL
        CONSTRAINT "FK_TriggerInvocations_Triggers_TriggerId"
            REFERENCES "Triggers"
            ON DELETE NO ACTION
            ON UPDATE CASCADE,
    "Timestamp" TIMESTAMP NOT NULL
);

CREATE INDEX "IX_TriggerInvocations_TriggerId"
    ON "TriggerInvocations" ("TriggerID");