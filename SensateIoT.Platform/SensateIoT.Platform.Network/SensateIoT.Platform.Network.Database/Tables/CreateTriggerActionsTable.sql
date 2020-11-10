---
--- Trigger actions data table.
---

CREATE TABLE "TriggerActions"
(
    "ID" BIGINT NOT NULL GENERATED ALWAYS AS IDENTITY (INCREMENT 1 START 1) CONSTRAINT "PK_TriggerActions" PRIMARY KEY,
    "TriggerID" BIGINT NOT NULL
        CONSTRAINT "FK_TriggerActions_Triggers_TriggerId"
            REFERENCES "Triggers"
            ON DELETE CASCADE,
    "Channel"   INTEGER NOT NULL,
    "Target"    VARCHAR(255),
    "Message"   TEXT NOT NULL,

    CONSTRAINT "Alternative_Key_TriggerActions"
        UNIQUE ("TriggerID", "Channel", "Target")
);
