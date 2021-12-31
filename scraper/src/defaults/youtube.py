from collections import OrderedDict

# Convert from query string to python tuples:
# sed -nr "s/&(.+)=(.+)/\('\1', '\2'\),/p" <FILE>

ANALYTICS_QUERY_STRING_LIST = [
	('entity_type', 'VIDEO'),
	('entity_id', '<USE_YOUR_VIDEO_ID_HERE>'),
	# You won't be able to export to CSV if you choose "since_publish"
	('time_period', '4_weeks'),
	('explore_type', 'TABLE_AND_CHART'),
	('metric', 'VIEWS'),
	('granularity', 'DAY'),
	('t_metrics', 'VIEWS'),
	('t_metrics', 'WATCH_TIME'),
	('t_metrics', 'SUBSCRIBERS_NET_CHANGE'),
	('t_metrics', 'VIDEO_THUMBNAIL_IMPRESSIONS'),
	('t_metrics', 'VIDEO_THUMBNAIL_IMPRESSIONS_VTR'),
	('v_metrics', 'VIEWS'),
	('v_metrics', 'WATCH_TIME'),
	('v_metrics', 'SUBSCRIBERS_NET_CHANGE'),
	('v_metrics', 'VIDEO_THUMBNAIL_IMPRESSIONS'),
	('v_metrics', 'VIDEO_THUMBNAIL_IMPRESSIONS_VTR'),
	('dimension', 'VIDEO'),
	('o_column', 'VIEWS'),
	('o_direction', 'ANALYTICS_ORDER_DIRECTION_DESC'),
]

ANALYTICS_QUERY_STRING_DICT = OrderedDict(ANALYTICS_QUERY_STRING_LIST)

NAVIGATOR_DEFAULT_OPTIONS = {
	
}
