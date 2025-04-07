-- SQL Script to add an admin account to AgentScrum database

-- Variables (modify these with your desired values)
DO $$
DECLARE
    admin_email VARCHAR := 'your-email@example.com';  -- Change this to your email
    admin_username VARCHAR := 'your-email@example.com';  -- Change this to your username (can be same as email)
    admin_password_hash VARCHAR := 'AQAAAAIAAYagAAAAEHxrk9Q+dJ0xQyuq+QKjGDc8W3YwZ+PR0z9tQZn1XQKM+NWyEgzCWRa+TbBKQJbQxw==';  -- This is a hashed password for 'Password123!'
    admin_id VARCHAR := gen_random_uuid()::text;
    admin_role_id VARCHAR := gen_random_uuid()::text;
    admin_role_name VARCHAR := 'Admin';
    normalized_email VARCHAR := upper(admin_email);
    normalized_username VARCHAR := upper(admin_username);
BEGIN
    -- 1. Check if the user already exists
    IF NOT EXISTS (SELECT 1 FROM "AspNetUsers" WHERE "NormalizedEmail" = normalized_email) THEN
        -- Insert the admin user
        INSERT INTO "AspNetUsers" (
            "Id", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", 
            "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", 
            "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", 
            "LockoutEnabled", "AccessFailedCount"
        ) VALUES (
            admin_id, admin_username, normalized_username, admin_email, normalized_email,
            TRUE, admin_password_hash, gen_random_uuid()::text, gen_random_uuid()::text,
            NULL, FALSE, FALSE, NULL, TRUE, 0
        );
        
        RAISE NOTICE 'Admin user created with ID: %', admin_id;
    ELSE
        -- Get the existing user ID
        SELECT "Id" INTO admin_id FROM "AspNetUsers" WHERE "NormalizedEmail" = normalized_email;
        RAISE NOTICE 'Admin user already exists with ID: %', admin_id;
    END IF;
    
    -- 2. Check if Admin role exists
    IF NOT EXISTS (SELECT 1 FROM "AspNetRoles" WHERE "NormalizedName" = upper(admin_role_name)) THEN
        -- Create Admin role
        INSERT INTO "AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
        VALUES (admin_role_id, admin_role_name, upper(admin_role_name), gen_random_uuid()::text);
        
        RAISE NOTICE 'Admin role created with ID: %', admin_role_id;
    ELSE
        -- Get the existing role ID
        SELECT "Id" INTO admin_role_id FROM "AspNetRoles" WHERE "NormalizedName" = upper(admin_role_name);
        RAISE NOTICE 'Admin role already exists with ID: %', admin_role_id;
    END IF;
    
    -- 3. Assign user to Admin role (if not already assigned)
    IF NOT EXISTS (SELECT 1 FROM "AspNetUserRoles" WHERE "UserId" = admin_id AND "RoleId" = admin_role_id) THEN
        INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
        VALUES (admin_id, admin_role_id);
        
        RAISE NOTICE 'User assigned to Admin role';
    ELSE
        RAISE NOTICE 'User already assigned to Admin role';
    END IF;
    
    -- 4. Add admin claims
    IF NOT EXISTS (SELECT 1 FROM "AspNetUserClaims" WHERE "UserId" = admin_id AND "ClaimType" = 'Permission' AND "ClaimValue" = 'ManageUsers') THEN
        INSERT INTO "AspNetUserClaims" ("UserId", "ClaimType", "ClaimValue")
        VALUES (admin_id, 'Permission', 'ManageUsers');
        
        RAISE NOTICE 'Added ManageUsers claim';
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM "AspNetUserClaims" WHERE "UserId" = admin_id AND "ClaimType" = 'Permission' AND "ClaimValue" = 'ViewReports') THEN
        INSERT INTO "AspNetUserClaims" ("UserId", "ClaimType", "ClaimValue")
        VALUES (admin_id, 'Permission', 'ViewReports');
        
        RAISE NOTICE 'Added ViewReports claim';
    END IF;
    
    IF NOT EXISTS (SELECT 1 FROM "AspNetUserClaims" WHERE "UserId" = admin_id AND "ClaimType" = 'Permission' AND "ClaimValue" = 'ManageSettings') THEN
        INSERT INTO "AspNetUserClaims" ("UserId", "ClaimType", "ClaimValue")
        VALUES (admin_id, 'Permission', 'ManageSettings');
        
        RAISE NOTICE 'Added ManageSettings claim';
    END IF;
    
    RAISE NOTICE 'Admin account setup complete!';
END $$; 