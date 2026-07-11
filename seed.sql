-- Dev seed data for LearnFlow.
-- Creates an admin user (admin@learnflow.local / admin123 — DEV ONLY, change in prod),
-- an "admin" role, and the permission set used by the frontend.
-- Run after the schema exists (dotnet ef database update):
--   psql "postgresql://learnflow:learnflow@localhost:5433/learnflow" -f seed.sql

BEGIN;

INSERT INTO "AdminUsers" ("Id", "Email", "PasswordHash", "Name", "IsActive", "CreatedAt", "UpdatedAt")
VALUES (
    'a0000000-0000-0000-0000-000000000001',
    'admin@learnflow.local',
    '$2b$10$nTGQKlHplGi9JsrSsZVoluwUiDeSc73kPatw..cdwPa/KOfotzUOa',
    'LearnFlow Admin',
    TRUE,
    NOW(), NOW()
)
ON CONFLICT DO NOTHING;

INSERT INTO "AdminRoles" ("Id", "Name", "Description", "CreatedAt", "UpdatedAt")
VALUES ('b0000000-0000-0000-0000-000000000001', 'admin', 'Full access', NOW(), NOW())
ON CONFLICT DO NOTHING;

INSERT INTO "AdminUserRoles" ("Id", "UserId", "RoleId", "CreatedAt")
VALUES (
    'c0000000-0000-0000-0000-000000000001',
    'a0000000-0000-0000-0000-000000000001',
    'b0000000-0000-0000-0000-000000000001',
    NOW()
)
ON CONFLICT DO NOTHING;

-- Permission codes exposed to the frontend as "{Resource}_{Action}"
INSERT INTO "AdminPermissions" ("Id", "Resource", "Action", "Description", "CreatedAt", "UpdatedAt")
VALUES
    ('d0000000-0000-0000-0000-000000000001', 'dashboard',   'view',   'View dashboard',     NOW(), NOW()),
    ('d0000000-0000-0000-0000-000000000002', 'courses',     'view',   'View courses',       NOW(), NOW()),
    ('d0000000-0000-0000-0000-000000000003', 'courses',     'create', 'Create courses',     NOW(), NOW()),
    ('d0000000-0000-0000-0000-000000000004', 'courses',     'update', 'Update courses',     NOW(), NOW()),
    ('d0000000-0000-0000-0000-000000000005', 'courses',     'delete', 'Delete courses',     NOW(), NOW()),
    ('d0000000-0000-0000-0000-000000000006', 'lessons',     'view',   'View lessons',       NOW(), NOW()),
    ('d0000000-0000-0000-0000-000000000007', 'lessons',     'create', 'Create lessons',     NOW(), NOW()),
    ('d0000000-0000-0000-0000-000000000008', 'lessons',     'update', 'Update lessons',     NOW(), NOW()),
    ('d0000000-0000-0000-0000-000000000009', 'lessons',     'delete', 'Delete lessons',     NOW(), NOW()),
    ('d0000000-0000-0000-0000-00000000000a', 'enrollments', 'view',   'View enrollments',   NOW(), NOW()),
    ('d0000000-0000-0000-0000-00000000000b', 'enrollments', 'create', 'Create enrollments', NOW(), NOW()),
    ('d0000000-0000-0000-0000-00000000000c', 'enrollments', 'update', 'Update enrollments', NOW(), NOW()),
    ('d0000000-0000-0000-0000-00000000000d', 'enrollments', 'delete', 'Delete enrollments', NOW(), NOW())
ON CONFLICT DO NOTHING;

INSERT INTO "AdminRolePermissions" ("Id", "RoleId", "PermissionId", "CreatedAt")
SELECT gen_random_uuid(), 'b0000000-0000-0000-0000-000000000001', p."Id", NOW()
FROM "AdminPermissions" p
WHERE NOT EXISTS (
    SELECT 1 FROM "AdminRolePermissions" rp
    WHERE rp."RoleId" = 'b0000000-0000-0000-0000-000000000001'
      AND rp."PermissionId" = p."Id"
);

COMMIT;
