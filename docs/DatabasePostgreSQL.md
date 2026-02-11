## การเชื่อม DatabasePostgreSQL

https://humane-math-026.notion.site/PostgreSQL-301e86c18ff58063bccde2bfabbe1cb9

## การใช้ Dbeaver

!! อย่าลืม .env

1. Start Database with Docker Compose:
   ```bash
   docker-compose up -d
   ```

2. Wait for Database to be ready (approximately 5-10 seconds)

3. Update Database schema:
   ```bash
   dotnet ef database update
   ```

4. Open Dbeaver แล้วขวาบนกด รูปปลั๊ก (new database connection)

5. กดเลือก postgreSQL

6. กด check "show database all"

7. host , database , username , password เหมือนใน .env

8. กด Finish