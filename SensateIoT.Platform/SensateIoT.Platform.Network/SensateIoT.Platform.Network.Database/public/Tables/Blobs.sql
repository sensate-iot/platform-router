CREATE TABLE "Blobs"
(
	"ID" BIGINT GENERATED ALWAYS AS IDENTITY CONSTRAINT "PK_Blobs" PRIMARY KEY,
	"SensorID" VARCHAR(24) NOT NULL,
	"FileName" TEXT NOT NULL,
	"Path" TEXT NOT NULL,
	"StorageType" INTEGER NOT NULL,
	"Timestamp" timestamp default '0001-01-01 00:00:00'::TIMESTAMP NOT NULL,
	"FileSize" BIGINT DEFAULT 0 NOT NULL
);

CREATE INDEX "IX_Blobs_SensorID"
	ON "Blobs" ("SensorID");

CREATE UNIQUE INDEX "IX_Blobs_SensorID_FileName"
	ON "Blobs" ("SensorID", "FileName");

