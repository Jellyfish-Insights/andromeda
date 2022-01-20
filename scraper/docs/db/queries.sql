-- Run with 'PGPASSWORD=root psql -f queries.sql -U postgres -h localhost -p 5433'

-- Table with videos grouped by account_name
SELECT
	account_name AS "Account name",
	date_trunc('minute', max(saved_time)) AS "Last scan",
	COUNT(DISTINCT tiktok_id) AS "# of videos"
FROM video_info
GROUP BY account_name ;

-- Some general statistics on the accounts observed
WITH cte AS
(
	SELECT 
	DISTINCT ON (tiktok_id)
	*
	FROM video_info
)
SELECT
	account_name AS "Account name",
	COUNT(*) AS "# of videos",
	ROUND(AVG(play_count), 0) AS "Avg play count",
	ROUND(AVG(play_count / EXTRACT(EPOCH FROM (NOW() - create_time))), 2) AS "Avg play / sec",
	ROUND(AVG(play_count / NULLIF(digg_count,0)), 1) AS "Avg play / like",
	ROUND(AVG(play_count / NULLIF(comment_count,0)), 0) AS "Avg play / comment",
	ROUND(AVG(play_count / NULLIF(share_count,0)), 0) AS "Avg play / share"
FROM cte
GROUP BY account_name
ORDER BY avg(play_count) DESC ;

-- How many videos scanned per second?

--Production mode, using --slow-mode
SELECT
	ROUND
	(
		COUNT(*) / ( EXTRACT(EPOCH FROM (MAX(saved_time) - MIN(saved_time))) / 60),
		2
	) AS "Videos / min"
FROM video_info
WHERE saved_time > '2021-12-20 23:45';
-- 4.20 videos / min

-- Development mode
SELECT
	ROUND
	(
		COUNT(*) / ( EXTRACT(EPOCH FROM (MAX(saved_time) - MIN(saved_time))) / 60),
		2
	) AS "Videos / min"
FROM video_info
WHERE saved_time < '2021-12-20 23:45';
-- 21.39 videos / min
