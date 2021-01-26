---
--- Trigger invocations table.
---

CREATE TABLE "TriggerInvocations"
(
    "ID"  BIGINT NOT NULL GENERATED ALWAYS AS IDENTITY (INCREMENT 1 START 1)
        CONSTRAINT "PK_TriggerInvocations" PRIMARY KEY,
    "TriggerID" BIGINT NOT NULL
        CONSTRAINT "FK_TriggerInvocations_TriggerActions_TriggerID"
        REFERENCES "Triggers"
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    "ActionID" BIGINT NOT NULL
        CONSTRAINT "FK_TriggerInvocations_TriggerActions_ActionID"
        REFERENCES "TriggerActions"
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    "Timestamp" TIMESTAMP NOT NULL
);

CREATE INDEX "IX_TriggerInvocations_ActionID"
    ON "TriggerInvocations" ("ActionID");