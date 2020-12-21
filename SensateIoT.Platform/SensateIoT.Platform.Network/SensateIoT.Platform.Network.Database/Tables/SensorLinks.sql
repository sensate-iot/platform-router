CREATE TABLE public."SensorLinks"
(
	"SensorId" TEXT NOT NULL,
	"UserId" TEXT NOT NULL,
	CONSTRAINT "PK_SensorLinks"
		PRIMARY KEY ("UserId", "SensorId")
);

CREATE INDEX "IX_SensorLinks_UserId"
	ON public."SensorLinks" ("UserId");

CREATE INDEX "IX_SensorLinks_SensorId"
	ON public."SensorLinks" ("SensorId");

