export interface ExternalLoginViewModel {
  email: string;
}
export enum YearApPermissionLevels {
  Admin = 0,
  Editor = 1,
  ReadOnly = 2,
}
export interface AuthStateInfo {
  name: string;
  role: YearApPermissionLevels;
}
export interface ChartObject {
  date: string;
  values: { [index: string]: number };
}
export interface TimeSeriesChartData {
  startDate: Date;
  endDate: Date;
  metric: string;
  chartObjectArray: ChartObject[];
  totalPerGroup: { [index: string]: number };
  totalOnPeriod: number;
}
export interface AllAccountsInfo {
  accounts: { [index: string]: AccountInfo };
}
export interface AccountInfo {
  role: YearApPermissionLevels;
  lastLogIn: Date;
  registrationDate: Date;
  lastUpdate: Date;
}
export interface SingleAccountEdit {
  flag: EditType;
  permission: YearApPermissionLevels;
  versionStamp: Date;
}
export interface AccountEdit {
  edits: { [index: string]: SingleAccountEdit };
}
export interface PersonaVersionEdits {
  edits: { [index: string]: PersonaVersionEdit };
}
export interface PersonaVersionEdit {
  archive?: boolean;
  addedAdSets?: string[];
  removedAdSets?: string[];
  updateDate: Date;
  flag: EditType;
}
export interface PersonaVersion {
  adSets: SourceObject[];
  id: string;
  version: string;
  name: string;
  archived: boolean;
  updateDate: Date;
}
export interface SourceObject {
  sourceId: string;
  sourceName: string;
  title: string;
  thumbnailLink: string;
  type: SourceObjectType;
  links: SourceLink[];
  lengthInSeconds?: number;
  publishedAt: Date;
  publishedStatus: boolean;
}
export interface SourceLink {
  type: SourceLinkType;
  link: string;
}
export enum SourceLinkType {
  Content = 0,
  Analytics = 1,
}
export enum SourceObjectType {
  Video = 0,
  Campaign = 1,
  AdSet = 2,
}
export enum ArchiveMode {
  UnArchived = 0,
  Archived = 1,
  All = 2,
}
export interface VideoEdits {
  edits: { [index: string]: VideoEdit };
}
export interface TagEdits {
  type: string;
  edits: { [index: string]: TagEdit };
}
export interface TagEdit {
  name?: string;
  updateDate: Date;
  flag: EditType;
}
export interface VideoEdit {
  title?: string;
  archive?: boolean;
  updateDate: Date;
  metaTags?: { [index: string]: string | null };
  addedGenericTags?: string[];
  removedGenericTags?: string[];
  addedCampaigns?: string[];
  removedCampaigns?: string[];
  addedVideos?: string[];
  removedVideos?: string[];
  flag: EditType;
}
export enum AddOrRemove {
  Add = 1,
  Remove = 2,
}
export enum EditType {
  New = 1,
  Update = 2,
  Delete = 3,
}
export interface Video {
  id: string;
  title: string;
  archived: boolean;
  updateDate: Date;
  playlists: string[];
  tags: Tag[];
  sources: Source[];
  thumbnailLink: string;
  publishedAt: Date;
}
export interface VideoMetric {
  id: string;
  totalMetrics: Metric[];
  metricsPerPersona?: PersonaMetric[];
}
export interface Source {
  sourceName: string;
  videosCount: number;
  sourceObjects: SourceObject[];
}
export interface Tag {
  type: string;
  value: string;
  updateDate: Date;
  color?: string;
}
export interface PersonaMetric {
  persona: string;
  metrics: Metric[];
}
export interface Metric {
  type: string;
  controllerType?: string;
  value: number;
}
export interface MetricInfo {
  typeId: MetricType;
  type: string;
  abbreviation: string;
  unit: string;
  unitSide: string;
  chartType: ChartType;
  pageType: string;
  markdownSource: string;
}
export enum ChartType {
  LINE = 0,
  BAR = 1,
}
export enum MetricType {
  Views = 0,
  AverageViewTime = 1,
  Comments = 2,
  DemographicsViewCount = 3,
  DemographicsViewTime = 4,
  Dislikes = 5,
  Impressions = 6,
  Likes = 7,
  Reactions = 8,
  Shares = 9,
  ViewTime = 10,
  Clicks = 11,
  ClickCost = 12,
  CostPerView = 13,
  CostPerClick = 14,
  CostPerEmailCapture = 15,
  CostPerEngagement = 16,
  CostPerImpression = 17,
  EmailCaptures = 18,
  EmailCaptureCost = 19,
  Engagements = 20,
  EngagementCost = 21,
  ImpressionCost = 22,
  Reach = 23,
  TotalCost = 24,
  ViewCost = 25,
}
export interface DemographicDataItem {
  groupName: string;
  values: { [index: string]: { [index: string]: number } };
  total: number;
}
export interface DemographicData {
  values: DemographicDataItem[];
  groups: string[];
}
export interface TimeSeriesDataGroup {
  groupName: string;
  values: number[];
}
export interface TimeSeries {
  dates: string[];
  values: TimeSeriesDataGroup[];
  totalTimeSeries: TimeSeriesDataGroup;
  totalPerGroup: { [index: string]: number };
  totalOnPeriod: number;
}
