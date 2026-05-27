CREATE TABLE IF NOT EXISTS roles (
    id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY ux_roles_name (name)
);

CREATE TABLE IF NOT EXISTS permissions (
    id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY ux_permissions_name (name)
);

CREATE TABLE IF NOT EXISTS modules (
    id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(100) NOT NULL,
    description VARCHAR(255) NULL,
    PRIMARY KEY (id),
    UNIQUE KEY ux_modules_name (name)
);

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS role_id INT NULL;

CREATE TABLE IF NOT EXISTS role_permissions (
    role_id INT NOT NULL,
    permission_id INT NOT NULL,
    PRIMARY KEY (role_id, permission_id),
    CONSTRAINT fk_role_permissions_role
        FOREIGN KEY (role_id) REFERENCES roles(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_role_permissions_permission
        FOREIGN KEY (permission_id) REFERENCES permissions(id)
        ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS role_modules (
    role_id INT NOT NULL,
    module_id INT NOT NULL,
    PRIMARY KEY (role_id, module_id),
    CONSTRAINT fk_role_modules_role
        FOREIGN KEY (role_id) REFERENCES roles(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_role_modules_module
        FOREIGN KEY (module_id) REFERENCES modules(id)
        ON DELETE CASCADE
);

INSERT INTO roles (id, name, description)
VALUES
    (1, 'ADMIN', 'Accesso completo'),
    (2, 'USER', 'Accesso standard')
ON DUPLICATE KEY UPDATE
    name = VALUES(name),
    description = VALUES(description);

INSERT INTO permissions (name, description)
VALUES
    ('users.read', 'Visualizzare utenti'),
    ('users.write', 'Creare e modificare utenti'),
    ('roles.read', 'Visualizzare ruoli'),
    ('roles.write', 'Creare, modificare ed eliminare ruoli'),
    ('crm.read', 'Visualizzare dati CRM'),
    ('crm.write', 'Creare e modificare dati CRM'),
    ('crm.delete', 'Eliminare dati CRM')
ON DUPLICATE KEY UPDATE
    description = VALUES(description);

INSERT INTO modules (name, description)
VALUES
    ('users', 'Gestione utenti'),
    ('roles', 'Gestione ruoli'),
    ('companies', 'Aziende'),
    ('contacts', 'Contatti'),
    ('tasks', 'Task'),
    ('files', 'File')
ON DUPLICATE KEY UPDATE
    description = VALUES(description);

INSERT IGNORE INTO role_permissions (role_id, permission_id)
SELECT 1, id FROM permissions;

INSERT IGNORE INTO role_modules (role_id, module_id)
SELECT 1, id FROM modules;

INSERT IGNORE INTO role_permissions (role_id, permission_id)
SELECT 2, id FROM permissions WHERE name IN ('crm.read', 'crm.write');

INSERT IGNORE INTO role_modules (role_id, module_id)
SELECT 2, id FROM modules WHERE name IN ('companies', 'contacts', 'tasks', 'files');

UPDATE users
SET role_id = 2
WHERE role_id IS NULL;

SET @fk_users_role_exists = (
    SELECT COUNT(*)
    FROM information_schema.TABLE_CONSTRAINTS
    WHERE CONSTRAINT_SCHEMA = DATABASE()
      AND TABLE_NAME = 'users'
      AND CONSTRAINT_NAME = 'fk_users_role'
      AND CONSTRAINT_TYPE = 'FOREIGN KEY'
);

SET @sql = IF(
    @fk_users_role_exists = 0,
    'ALTER TABLE users ADD CONSTRAINT fk_users_role FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE SET NULL',
    'SELECT 1'
);

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;
