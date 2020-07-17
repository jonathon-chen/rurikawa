use serde::{Deserialize, Serialize};
use serde_derive::*;

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct JudgeToml {
    pub id: String,
}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct JudgeConfig {}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub enum ImageConfig {
    Remote(String),
    Dockerfile(String),
}

#[derive(Serialize, Deserialize, Debug, Clone)]
pub struct JobConfig {
    pub time_limit: usize,
    pub mem_limit: usize,
    pub before_exec: Vec<Vec<String>>,
    pub exec: Vec<String>,
}