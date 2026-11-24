using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LS.Persistence.Migrations.Shared
{
    /// <inheritdoc />
    public partial class QuartzPersistentStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Quartz owns these tables outside the EF model. Create the persistent job store only when it is
            // missing, then apply the idempotent Quartz 4.6 schema changes. Ported from StarterTemplate.
            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.QRTZ_JOB_DETAILS', N'U') IS NULL
                BEGIN
                CREATE TABLE [dbo].[QRTZ_CALENDARS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [CALENDAR_NAME] nvarchar(200) NOT NULL,
                  [CALENDAR] varbinary(max) NOT NULL
                );

                CREATE TABLE [dbo].[QRTZ_CRON_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [CRON_EXPRESSION] nvarchar(120) NOT NULL,
                  [TIME_ZONE_ID] nvarchar(80)
                );

                CREATE TABLE [dbo].[QRTZ_FIRED_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [ENTRY_ID] nvarchar(140) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [INSTANCE_NAME] nvarchar(200) NOT NULL,
                  [FIRED_TIME] bigint NOT NULL,
                  [SCHED_TIME] bigint NOT NULL,
                  [PRIORITY] int NOT NULL,
                  [STATE] nvarchar(16) NOT NULL,
                  [JOB_NAME] nvarchar(150) NULL,
                  [JOB_GROUP] nvarchar(150) NULL,
                  [IS_NONCONCURRENT] bit NULL,
                  [REQUESTS_RECOVERY] bit NULL,
                  [EXECUTION_GROUP] nvarchar(200) NULL
                );

                CREATE TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL
                );

                CREATE TABLE [dbo].[QRTZ_SCHEDULER_STATE] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [INSTANCE_NAME] nvarchar(200) NOT NULL,
                  [LAST_CHECKIN_TIME] bigint NOT NULL,
                  [CHECKIN_INTERVAL] bigint NOT NULL
                );

                CREATE TABLE [dbo].[QRTZ_LOCKS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [LOCK_NAME] nvarchar(40) NOT NULL
                );

                CREATE TABLE [dbo].[QRTZ_JOB_DETAILS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [JOB_NAME] nvarchar(150) NOT NULL,
                  [JOB_GROUP] nvarchar(150) NOT NULL,
                  [DESCRIPTION] nvarchar(250) NULL,
                  [JOB_CLASS_NAME] nvarchar(250) NOT NULL,
                  [IS_DURABLE] bit NOT NULL,
                  [IS_NONCONCURRENT] bit NOT NULL,
                  [IS_UPDATE_DATA] bit NOT NULL,
                  [REQUESTS_RECOVERY] bit NOT NULL,
                  [JOB_DATA] varbinary(max) NULL
                );

                CREATE TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [REPEAT_COUNT] int NOT NULL,
                  [REPEAT_INTERVAL] bigint NOT NULL,
                  [TIMES_TRIGGERED] int NOT NULL
                );

                CREATE TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [STR_PROP_1] nvarchar(512) NULL,
                  [STR_PROP_2] nvarchar(512) NULL,
                  [STR_PROP_3] nvarchar(512) NULL,
                  [INT_PROP_1] int NULL,
                  [INT_PROP_2] int NULL,
                  [LONG_PROP_1] bigint NULL,
                  [LONG_PROP_2] bigint NULL,
                  [DEC_PROP_1] numeric(13,4) NULL,
                  [DEC_PROP_2] numeric(13,4) NULL,
                  [BOOL_PROP_1] bit NULL,
                  [BOOL_PROP_2] bit NULL,
                  [TIME_ZONE_ID] nvarchar(80) NULL
                );

                CREATE TABLE [dbo].[QRTZ_BLOB_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [BLOB_DATA] varbinary(max) NULL
                );

                CREATE TABLE [dbo].[QRTZ_TRIGGERS] (
                  [SCHED_NAME] nvarchar(120) NOT NULL,
                  [TRIGGER_NAME] nvarchar(150) NOT NULL,
                  [TRIGGER_GROUP] nvarchar(150) NOT NULL,
                  [JOB_NAME] nvarchar(150) NOT NULL,
                  [JOB_GROUP] nvarchar(150) NOT NULL,
                  [DESCRIPTION] nvarchar(250) NULL,
                  [NEXT_FIRE_TIME] bigint NULL,
                  [PREV_FIRE_TIME] bigint NULL,
                  [PRIORITY] int NULL,
                  [TRIGGER_STATE] nvarchar(16) NOT NULL,
                  [TRIGGER_TYPE] nvarchar(8) NOT NULL,
                  [START_TIME] bigint NOT NULL,
                  [END_TIME] bigint NULL,
                  [CALENDAR_NAME] nvarchar(200) NULL,
                  [MISFIRE_INSTR] int NULL,
                  [MISFIRE_ORIG_FIRE_TIME] bigint NULL,
                  [EXECUTION_GROUP] nvarchar(200) NULL,
                  [PREFERRED_NODE] nvarchar(200) NULL,
                  [PREFERRED_NODE_AUTO] bit NOT NULL DEFAULT 0,
                  [JOB_DATA] varbinary(max) NULL
                );

                ALTER TABLE [dbo].[QRTZ_CALENDARS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_CALENDARS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [CALENDAR_NAME]
                  );

                ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_CRON_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );

                ALTER TABLE [dbo].[QRTZ_FIRED_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_FIRED_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [ENTRY_ID]
                  );

                ALTER TABLE [dbo].[QRTZ_PAUSED_TRIGGER_GRPS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_PAUSED_TRIGGER_GRPS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_GROUP]
                  );

                ALTER TABLE [dbo].[QRTZ_SCHEDULER_STATE] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_SCHEDULER_STATE] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [INSTANCE_NAME]
                  );

                ALTER TABLE [dbo].[QRTZ_LOCKS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_LOCKS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [LOCK_NAME]
                  );

                ALTER TABLE [dbo].[QRTZ_JOB_DETAILS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_JOB_DETAILS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [JOB_NAME],
                    [JOB_GROUP]
                  );

                ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_SIMPLE_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );

                ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_SIMPROP_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );

                ALTER TABLE [dbo].[QRTZ_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );

                ALTER TABLE [dbo].[QRTZ_BLOB_TRIGGERS] WITH NOCHECK ADD
                  CONSTRAINT [PK_QRTZ_BLOB_TRIGGERS] PRIMARY KEY  CLUSTERED
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  );

                ALTER TABLE [dbo].[QRTZ_CRON_TRIGGERS] ADD
                  CONSTRAINT [FK_QRTZ_CRON_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) ON DELETE CASCADE;

                ALTER TABLE [dbo].[QRTZ_SIMPLE_TRIGGERS] ADD
                  CONSTRAINT [FK_QRTZ_SIMPLE_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
                  (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) ON DELETE CASCADE;

                ALTER TABLE [dbo].[QRTZ_SIMPROP_TRIGGERS] ADD
                  CONSTRAINT [FK_QRTZ_SIMPROP_TRIGGERS_QRTZ_TRIGGERS] FOREIGN KEY
                  (
                	[SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) REFERENCES [dbo].[QRTZ_TRIGGERS] (
                    [SCHED_NAME],
                    [TRIGGER_NAME],
                    [TRIGGER_GROUP]
                  ) ON DELETE CASCADE;

                ALTER TABLE [dbo].[QRTZ_TRIGGERS] ADD
                  CONSTRAINT [FK_QRTZ_TRIGGERS_QRTZ_JOB_DETAILS] FOREIGN KEY
                  (
                    [SCHED_NAME],
                    [JOB_NAME],
                    [JOB_GROUP]
                  ) REFERENCES [dbo].[QRTZ_JOB_DETAILS] (
                    [SCHED_NAME],
                    [JOB_NAME],
                    [JOB_GROUP]
                  );

                -- Create indexes
                CREATE INDEX [IDX_QRTZ_T_G_J]                 ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, JOB_GROUP, JOB_NAME);
                CREATE INDEX [IDX_QRTZ_T_C]                   ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, CALENDAR_NAME);

                CREATE INDEX [IDX_QRTZ_T_N_G_STATE]           ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_STATE);
                CREATE INDEX [IDX_QRTZ_T_STATE]               ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE);
                CREATE INDEX [IDX_QRTZ_T_N_STATE]             ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_NAME, TRIGGER_GROUP, TRIGGER_STATE);
                CREATE INDEX [IDX_QRTZ_T_NEXT_FIRE_TIME]      ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, NEXT_FIRE_TIME);
                CREATE INDEX [IDX_QRTZ_T_NFT_ST]              ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, TRIGGER_STATE, NEXT_FIRE_TIME);
                CREATE INDEX [IDX_QRTZ_T_NFT_ST_MISFIRE]      ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_STATE);
                CREATE INDEX [IDX_QRTZ_T_NFT_ST_MISFIRE_GRP]  ON [dbo].[QRTZ_TRIGGERS](SCHED_NAME, MISFIRE_INSTR, NEXT_FIRE_TIME, TRIGGER_GROUP, TRIGGER_STATE);

                CREATE INDEX [IDX_QRTZ_FT_INST_JOB_REQ_RCVRY] ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, INSTANCE_NAME, REQUESTS_RECOVERY);
                CREATE INDEX [IDX_QRTZ_FT_G_J]                ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, JOB_GROUP, JOB_NAME);
                CREATE INDEX [IDX_QRTZ_FT_G_T]                ON [dbo].[QRTZ_FIRED_TRIGGERS](SCHED_NAME, TRIGGER_GROUP, TRIGGER_NAME);
                END;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'dbo.QRTZ_PAUSED_JOB_GRPS', N'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.QRTZ_PAUSED_JOB_GRPS (
                        SCHED_NAME nvarchar(120) NOT NULL,
                        JOB_GROUP nvarchar(150) NOT NULL,
                        CONSTRAINT PK_QRTZ_PAUSED_JOB_GRPS PRIMARY KEY (SCHED_NAME, JOB_GROUP)
                    );
                END;

                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'MISFIRE_ORIG_FIRE_TIME') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD MISFIRE_ORIG_FIRE_TIME bigint NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'EXECUTION_GROUP') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD EXECUTION_GROUP nvarchar(200) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'PREFERRED_NODE') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD PREFERRED_NODE nvarchar(200) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'PREFERRED_NODE_AUTO') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD PREFERRED_NODE_AUTO bit NOT NULL DEFAULT (0);
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'RETRY_POLICY') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD RETRY_POLICY nvarchar(250) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'RETRY_ATTEMPT') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD RETRY_ATTEMPT int NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'CONTINUES_TRIGGER_NAME') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD CONTINUES_TRIGGER_NAME nvarchar(150) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'CONTINUES_TRIGGER_GROUP') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD CONTINUES_TRIGGER_GROUP nvarchar(150) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'CONTINUATION_CONDITION') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD CONTINUATION_CONDITION int NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'OVERLAP_POLICY') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD OVERLAP_POLICY int NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'PAUSE_REASON') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD PAUSE_REASON nvarchar(250) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'PAUSED_BY') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD PAUSED_BY nvarchar(200) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'PAUSED_AT') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD PAUSED_AT bigint NULL;
                IF COL_LENGTH(N'dbo.QRTZ_TRIGGERS', N'MISFIRE_THRESHOLD') IS NULL
                    ALTER TABLE dbo.QRTZ_TRIGGERS ADD MISFIRE_THRESHOLD bigint NULL;

                IF COL_LENGTH(N'dbo.QRTZ_FIRED_TRIGGERS', N'EXECUTION_GROUP') IS NULL
                    ALTER TABLE dbo.QRTZ_FIRED_TRIGGERS ADD EXECUTION_GROUP nvarchar(200) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_FIRED_TRIGGERS', N'PROGRESS') IS NULL
                    ALTER TABLE dbo.QRTZ_FIRED_TRIGGERS ADD PROGRESS int NULL;
                IF COL_LENGTH(N'dbo.QRTZ_FIRED_TRIGGERS', N'PROGRESS_MESSAGE') IS NULL
                    ALTER TABLE dbo.QRTZ_FIRED_TRIGGERS ADD PROGRESS_MESSAGE nvarchar(250) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_FIRED_TRIGGERS', N'INTERRUPT_REQUESTED_TIME') IS NULL
                    ALTER TABLE dbo.QRTZ_FIRED_TRIGGERS ADD INTERRUPT_REQUESTED_TIME bigint NULL;

                IF COL_LENGTH(N'dbo.QRTZ_PAUSED_TRIGGER_GRPS', N'PAUSE_REASON') IS NULL
                    ALTER TABLE dbo.QRTZ_PAUSED_TRIGGER_GRPS ADD PAUSE_REASON nvarchar(250) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_PAUSED_TRIGGER_GRPS', N'PAUSED_BY') IS NULL
                    ALTER TABLE dbo.QRTZ_PAUSED_TRIGGER_GRPS ADD PAUSED_BY nvarchar(200) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_PAUSED_TRIGGER_GRPS', N'PAUSED_AT') IS NULL
                    ALTER TABLE dbo.QRTZ_PAUSED_TRIGGER_GRPS ADD PAUSED_AT bigint NULL;

                IF COL_LENGTH(N'dbo.QRTZ_PAUSED_JOB_GRPS', N'PAUSE_REASON') IS NULL
                    ALTER TABLE dbo.QRTZ_PAUSED_JOB_GRPS ADD PAUSE_REASON nvarchar(250) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_PAUSED_JOB_GRPS', N'PAUSED_BY') IS NULL
                    ALTER TABLE dbo.QRTZ_PAUSED_JOB_GRPS ADD PAUSED_BY nvarchar(200) NULL;
                IF COL_LENGTH(N'dbo.QRTZ_PAUSED_JOB_GRPS', N'PAUSED_AT') IS NULL
                    ALTER TABLE dbo.QRTZ_PAUSED_JOB_GRPS ADD PAUSED_AT bigint NULL;

                IF COL_LENGTH(N'dbo.QRTZ_SCHEDULER_STATE', N'QUARTZ_VERSION') IS NULL
                    ALTER TABLE dbo.QRTZ_SCHEDULER_STATE ADD QUARTZ_VERSION nvarchar(20) NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Forward-only: dropping the job store would discard scheduled and in-flight jobs.
        }
    }
}
